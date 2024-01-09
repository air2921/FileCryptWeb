using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using webapi.DB;
using webapi.DTO;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
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

        private readonly ILogger<AuthRegistrationController> _logger;
        private readonly ICreate<UserModel> _userCreate;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly IEmailSender _email;
        private readonly IPasswordManager _passwordManager;
        private readonly IValidation _validation;
        private readonly FileCryptDbContext _dbContext;

        public AuthRegistrationController(
            ILogger<AuthRegistrationController> logger,
            ICreate<UserModel> userCreate,
            IGenerateSixDigitCode generateCode,
            IEmailSender email,
            IPasswordManager passwordManager,
            IValidation validation,
            FileCryptDbContext dbContext)
        {
            _logger = logger;
            _userCreate = userCreate;
            _generateCode = generateCode;
            _email = email;
            _passwordManager = passwordManager;
            _validation = validation;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registration([FromBody] UserModel userModel)
        {
            try
            {
                var email = userModel.email.ToLowerInvariant();

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
                if (user is not null)
                    return StatusCode(409, new { message = AccountErrorMessage.UserExists });

                if (!Regex.IsMatch(userModel.password, Validation.Password))
                    return StatusCode(400, new { message = AccountErrorMessage.InvalidFormatPassword });

                int code = _generateCode.GenerateSixDigitCode();

                string password = _passwordManager.HashingPassword(userModel.password);
                _logger.LogInformation("Password was hashed");

                HttpContext.Session.SetString(EMAIL, email);
                HttpContext.Session.SetString(PASSWORD, password);
                HttpContext.Session.SetString(USERNAME, userModel.username);
                HttpContext.Session.SetString(ROLE, Role.User.ToString());
                HttpContext.Session.SetInt32(email, code);
                _logger.LogInformation("Data was saved in user session");

                var emailDto = new EmailDto
                {
                    username = userModel.username,
                    email = userModel.email,
                    subject = EmailMessage.VerifyEmailHeader,
                    message = EmailMessage.VerifyEmailBody + code
                };

                await _email.SendMessage(emailDto);
                _logger.LogInformation($"Email was sended on {email} (1-st step)");

                return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAccount([FromQuery] int code)
        {
            string? email = HttpContext.Session.GetString(EMAIL);
            string? password = HttpContext.Session.GetString(PASSWORD);
            string? username = HttpContext.Session.GetString(USERNAME);
            string? role = HttpContext.Session.GetString(ROLE);
            int correctCode = (int)HttpContext.Session.GetInt32(email);

            if (email is null || password is null || username is null || role is null)
                return StatusCode(422, new { message = AccountErrorMessage.NullUserData });

            _logger.LogInformation("User data was succesfully received from session (not null anything)");

            try
            {
                if (!_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!code.Equals(correctCode))
                    return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

                var userModel = new UserModel { email = email, password = password, username = username, role = role };
                await _userCreate.Create(userModel);
                _logger.LogInformation("User was added in db");

                DeleteSessionData(email);
                _logger.LogInformation("User data deleted from session");

                return StatusCode(201, new { userModel });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                DeleteSessionData(email);
                _logger.LogInformation("User data deleted from session");

                return StatusCode(500, new { message = AccountErrorMessage.Error });
            }
        }

        private void DeleteSessionData(string email)
        {
            HttpContext.Session.Remove(email);
            HttpContext.Session.Remove(EMAIL);
            HttpContext.Session.Remove(PASSWORD);
            HttpContext.Session.Remove(USERNAME);
            HttpContext.Session.Remove(ROLE);
        }
    }
}
