using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    [ValidateAntiForgeryToken]
    public class AuthRegistrationController : ControllerBase
    {
        private const string EMAIL = "Email";
        private const string PASSWORD = "Password";
        private const string USERNAME = "Username";
        private const string ROLE = "Role";
        private const string IS_2FA = "2FA";
        private const string CODE = "Code";

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IGenerateKey _generateKey;
        private readonly IEncryptKey _encrypt;
        private readonly ILogger<AuthRegistrationController> _logger;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly IEmailSender _email;
        private readonly IPasswordManager _passwordManager;
        private readonly byte[] secretKey;

        public AuthRegistrationController(
            ILogger<AuthRegistrationController> logger,
            IGenerateSixDigitCode generateCode,
            IEmailSender email,
            IPasswordManager passwordManager,
            IRepository<UserModel> userRepository,
            IRepository<KeyModel> keyRepository,
            IRepository<TokenModel> tokenRepository,
            IConfiguration configuration,
            IGenerateKey generateKey,
            IEncryptKey encrypt)
        {
            _logger = logger;
            _generateCode = generateCode;
            _email = email;
            _passwordManager = passwordManager;

            _userRepository = userRepository;
            _keyRepository = keyRepository;
            _tokenRepository = tokenRepository;
            _configuration = configuration;
            _generateKey = generateKey;
            _encrypt = encrypt;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registration([FromBody] RegisterDTO userDTO)
        {
            try
            {
                var email = userDTO.email.ToLowerInvariant();
                int code = _generateCode.GenerateSixDigitCode();

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
                if (user is not null)
                    return StatusCode(409, new { message = AccountErrorMessage.UserExists });

                if (!Regex.IsMatch(userDTO.password, Validation.Password))
                    return StatusCode(400, new { message = AccountErrorMessage.InvalidFormatPassword });

                if (!Regex.IsMatch(userDTO.username, Validation.Username))
                    return StatusCode(400, new { message = AccountErrorMessage.InvalidFormatUsername });

                string password = _passwordManager.HashingPassword(userDTO.password);

                await _email.SendMessage(new EmailDto
                {
                    username = userDTO.username,
                    email = userDTO.email,
                    subject = EmailMessage.VerifyEmailHeader,
                    message = EmailMessage.VerifyEmailBody + code
                });
                _logger.LogInformation($"Email was sended on {email} (1-st step)");

                HttpContext.Session.SetString(EMAIL, email);
                HttpContext.Session.SetString(PASSWORD, password);
                HttpContext.Session.SetString(USERNAME, userDTO.username);
                HttpContext.Session.SetString(ROLE, Role.User.ToString());
                HttpContext.Session.SetString(IS_2FA, userDTO.is_2fa_enabled.ToString());
                HttpContext.Session.SetString(CODE, _passwordManager.HashingPassword(code.ToString()));

                _logger.LogInformation("Data was saved in user session");

                return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
            }
            catch (SmtpClientException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAccount([FromQuery] int code)
        {
            string? email = HttpContext.Session.GetString(EMAIL);
            string? password = HttpContext.Session.GetString(PASSWORD);
            string? username = HttpContext.Session.GetString(USERNAME);
            string? role = HttpContext.Session.GetString(ROLE);
            string? correctCode = HttpContext.Session.GetString(CODE);
            string? flag_2fa = HttpContext.Session.GetString(IS_2FA);

            if (email is null || password is null || username is null || role is null || correctCode is null || flag_2fa is null)
                return StatusCode(422, new { message = AccountErrorMessage.NullUserData });

            _logger.LogInformation("User data was succesfully received from session (not null anything)");

            try
            {
                bool IsCorrect = _passwordManager.CheckPassword(code.ToString(), correctCode);
                if (!IsCorrect)
                    return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

                var id = await _userRepository.Add(new UserModel
                {
                    email = email,
                    password = password,
                    username = username,
                    role = role,
                    is_2fa_enabled = bool.Parse(flag_2fa),
                    is_blocked = false
                }, e => e.id);

                await _tokenRepository.Add(new TokenModel
                {
                    user_id = id,
                    refresh_token = Guid.NewGuid().ToString(),
                    expiry_date = DateTime.UtcNow
                });

                await _keyRepository.Add(new KeyModel
                {
                    user_id = id,
                    private_key = await _encrypt.EncryptionKeyAsync(_generateKey.GenerateKey(), secretKey)
                });

                _logger.LogInformation("User was added in db");

                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            finally
            {
                HttpContext.Session.Clear();
                _logger.LogInformation("User data deleted from session");
            }
        }
    }
}
