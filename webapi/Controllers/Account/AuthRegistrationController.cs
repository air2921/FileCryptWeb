using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.Cryptography;
using webapi.DB;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

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

        #region fields and constructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly FileCryptDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IGenerate _generate;
        private readonly ICypherKey _encrypt;
        private readonly ILogger<AuthRegistrationController> _logger;
        private readonly IEmailSender _email;
        private readonly IPasswordManager _passwordManager;
        private readonly byte[] secretKey;

        public AuthRegistrationController(
            ILogger<AuthRegistrationController> logger,
            IEmailSender email,
            IPasswordManager passwordManager,
            IRepository<UserModel> userRepository,
            IRepository<KeyModel> keyRepository,
            IRepository<TokenModel> tokenRepository,
            FileCryptDbContext dbContext,
            IConfiguration configuration,
            IGenerate generate,
            IEnumerable<ICypherKey> cypherKeys)
        {
            _logger = logger;
            _email = email;
            _passwordManager = passwordManager;
            _userRepository = userRepository;
            _keyRepository = keyRepository;
            _tokenRepository = tokenRepository;
            _dbContext = dbContext;
            _configuration = configuration;
            _generate = generate;
            _encrypt = cypherKeys.FirstOrDefault(k => k.GetType().GetCustomAttribute<ImplementationKeyAttribute>()?.Key == "Encrypt");
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        #endregion

        [HttpPost("register")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Registration([FromBody] RegisterDTO userDTO)
        {
            try
            {
                var email = userDTO.email.ToLowerInvariant();
                int code = _generate.GenerateSixDigitCode();

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
                if (user is not null)
                    return StatusCode(409, new { message = Message.USER_EXISTS });

                if (!Regex.IsMatch(userDTO.password, Validation.Password) || !Regex.IsMatch(userDTO.username, Validation.Username))
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                await SendMessage(userDTO.username, userDTO.email, code);
                SetSession(HttpContext, new SessionObject
                {
                    Email = email,
                    Password = userDTO.password,
                    Username = userDTO.username,
                    Role = Role.User.ToString(),
                    Flag2Fa = userDTO.is_2fa_enabled,
                    Code = code.ToString()
                });

                return StatusCode(200, new { message = Message.EMAIL_SENT });
            }
            catch (SmtpClientException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("verify")]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> VerifyAccount([FromQuery] int code)
        {
            try
            {
                var session = GetSession(HttpContext);

                bool IsCorrect = _passwordManager.CheckPassword(code.ToString(), session.Code);
                if (!IsCorrect)
                    return StatusCode(422, new { message = Message.INCORRECT });

                await DbTransaction(session.Email, session.Password, session.Username, session.Role, session.Flag2Fa);

                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task DbTransaction(string email, string password, string username, string role, bool flag2fa)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var id = await _userRepository.Add(new UserModel
                {
                    email = email,
                    password = password,
                    username = username,
                    role = role,
                    is_2fa_enabled = flag2fa,
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
                    private_key = await _encrypt.CypherKeyAsync(_generate.GenerateKey(), secretKey)
                });

                await transaction.CommitAsync();
            }
            catch (EntityNotCreatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [Helper]
        private async Task SendMessage(string username, string email, int code)
        {
            try
            {
                await _email.SendMessage(new EmailDto
                {
                    username = username,
                    email = email,
                    subject = EmailMessage.VerifyEmailHeader,
                    message = EmailMessage.VerifyEmailBody + code
                });
            }
            catch (SmtpClientException)
            {
                throw;
            }
        }

        [Helper]
        private void SetSession(HttpContext context, SessionObject session)
        {
            context.Session.SetString(EMAIL, session.Email);
            context.Session.SetString(PASSWORD, _passwordManager.HashingPassword(session.Password));
            context.Session.SetString(USERNAME, session.Username);
            context.Session.SetString(ROLE, session.Role);
            context.Session.SetString(IS_2FA, session.Flag2Fa.ToString());
            context.Session.SetString(CODE, _passwordManager.HashingPassword(session.Code));
        }

        [Helper]
        private SessionObject GetSession(HttpContext context)
        {
            string? email = context.Session.GetString(EMAIL);
            string? password = context.Session.GetString(PASSWORD);
            string? username = context.Session.GetString(USERNAME);
            string? role = context.Session.GetString(ROLE);
            string? correctCode = context.Session.GetString(CODE);
            string? flag_2fa = context.Session.GetString(IS_2FA);

            if (email is null || password is null || username is null || role is null || correctCode is null || flag_2fa is null)
                throw new ArgumentNullException(Message.ERROR);

            return new SessionObject 
            { 
                Email = email,
                Password = password,
                Username = username,
                Role = role,
                Flag2Fa = bool.Parse(flag_2fa),
                Code = correctCode
            };
        }

        [AuxiliaryObject]
        private class SessionObject
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }
            public bool Flag2Fa { get; set; }
            public string Code { get; set; }

        }
    }
}
