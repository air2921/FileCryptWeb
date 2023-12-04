using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using webapi.DB;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    public class AuthRegistrationController : ControllerBase
    {
        private readonly ILogger<AuthRegistrationController> _logger;
        private readonly ICreate<UserModel> _userCreate;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly IEmailSender<UserModel> _email;
        private readonly IPasswordManager _passwordManager;
        private readonly IValidation _validation;
        private readonly FileCryptDbContext _dbContext;

        public AuthRegistrationController(
            ILogger<AuthRegistrationController> logger,
            ICreate<UserModel> userCreate,
            IGenerateSixDigitCode generateCode,
            IEmailSender<UserModel> email,
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
            var email = userModel.email.ToLowerInvariant();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
            if (user is not null)
                return StatusCode(409, new { message = AccountErrorMessage.UserExists });

            if (!Regex.IsMatch(userModel.password_hash, Validation.Password))
                return StatusCode(400, new { message = AccountErrorMessage.InvalidFormatPassword });

            int code = _generateCode.GenerateSixDigitCode();
            string messageHeader = EmailMessage.VerifyEmailHeader;
            string messageBody = EmailMessage.VerifyEmailBody + code;

            string password = _passwordManager.HashingPassword(userModel.password_hash);

            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("Password", password);
            HttpContext.Session.SetString("Username", userModel.username);
            HttpContext.Session.SetString("Role", Role.User.ToString());
            HttpContext.Session.SetString(email, code.ToString());

            await _email.SendMessage(userModel, messageHeader, messageBody);

            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAccount([FromQuery] int code)
        {
            string? Email = HttpContext.Session.GetString("Email");
            string? Password = HttpContext.Session.GetString("Password");
            string? Username = HttpContext.Session.GetString("Username");
            string? Role = HttpContext.Session.GetString("Role");

            if (Email is null || Password is null || Username is null || Role is null)
                return StatusCode(422, new { message = AccountErrorMessage.NullUserData });

            string? savedcode = HttpContext.Session.GetString(Email);
            if (string.IsNullOrWhiteSpace(savedcode))
                return StatusCode(422, new { message = AccountErrorMessage.VerifyCodeNull });

            try
            {
                int correctCode = int.Parse(savedcode);

                if (!_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!code.Equals(correctCode))
                    return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

                var userModel = new UserModel { email = Email, password_hash = Password, username = Username, role = Role };
                await _userCreate.Create(userModel);

                HttpContext.Session.Remove(Email);
                HttpContext.Session.Remove("Email");
                HttpContext.Session.Remove("Password");
                HttpContext.Session.Remove("Username");
                HttpContext.Session.Remove("Role");

                return StatusCode(201, new { userModel });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());

                HttpContext.Session.Remove(Email);
                HttpContext.Session.Remove("Email");
                HttpContext.Session.Remove("Password");
                HttpContext.Session.Remove("Username");

                return StatusCode(500, new { message = AccountErrorMessage.Error });
            }
        }
    }
}
