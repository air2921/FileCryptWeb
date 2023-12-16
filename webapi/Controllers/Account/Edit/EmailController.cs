using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/email")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private const string EMAIL = "Email";

        private readonly FileCryptDbContext _dbContext;
        private readonly IUpdate<UserModel> _update;
        private readonly IEmailSender<UserModel> _email;
        private readonly ILogger<EmailController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;

        public EmailController(
            FileCryptDbContext dbContext,
            IUpdate<UserModel> update,
            IEmailSender<UserModel> email,
            ILogger<EmailController> logger,
            IPasswordManager passwordManager,
            IGenerateSixDigitCode generateCode,
            ITokenService tokenService,
            IUserInfo userInfo,
            IValidation validation)
        {
            _dbContext = dbContext;
            _update = update;
            _email = email;
            _logger = logger;
            _passwordManager = passwordManager;
            _generateCode = generateCode;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _validation = validation;
        }

        [HttpPost("old")]
        public async Task<IActionResult> StartEmailChangeProcess([FromBody] UserModel userModel)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == _userInfo.Email);
            if (user is null)
            {
                _logger.LogWarning($"Non-existent user {_userInfo.Username}#{_userInfo.UserId} was requested to authorized endpoint.\nTrying delete tokens from cookie");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(404);
            }

            bool IsCorrect = _passwordManager.CheckPassword(userModel.password_hash, user.password_hash);
            if (!IsCorrect)
                return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

            int code = _generateCode.GenerateSixDigitCode();
            string messageHeader = EmailMessage.ConfirmOldEmailHeader;
            string message = EmailMessage.ConfirmOldEmailBody + code;

            var newUserModel = new UserModel { username = _userInfo.Username, email = _userInfo.Email };
            await _email.SendMessage(newUserModel, messageHeader, message);

            HttpContext.Session.SetInt32(_userInfo.Email, code);
            _logger.LogInformation($"Code was saved in user session {_userInfo.Username}#{_userInfo.UserId}");

            _logger.LogInformation($"Email to {_userInfo.Username}#{_userInfo.UserId} was sended on {_userInfo.Email} (1-st step)");
            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
        }

        [HttpPost("confirm/old")]
        public IActionResult ConfirmOldEmail([FromQuery] int code)
        {
            int correctCode = (int)HttpContext.Session.GetInt32(_userInfo.Email);
            _logger.LogInformation($"Code were received from user session {_userInfo.Username}#{_userInfo.UserId}. code: {correctCode}");

            if (!_validation.IsSixDigit(correctCode))
                return StatusCode(500, new { message = AccountErrorMessage.Error });

            if (!code.Equals(correctCode))
                return StatusCode(401, new { message = AccountErrorMessage.CodeIncorrect });

            _logger.LogInformation($"User {_userInfo.Username}#{_userInfo.UserId} confirmed code (2-nd step)");
            return StatusCode(201, new { message = AccountSuccessMessage.OldEmailConfirmed });
        }

        [HttpPost("new")]
        public async Task<IActionResult> SendEmailVerificationCode([FromBody] UserModel userModel)
        {
            string email = userModel.email.ToLowerInvariant();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
            if (user is not null)
                return StatusCode(409, new { message = AccountErrorMessage.UserExists });

            int code = _generateCode.GenerateSixDigitCode();
            string messageHeader = EmailMessage.ConfirmNewEmailHeader;
            string message = EmailMessage.ConfirmNewEmailBody + code;

            var newUserModel = new UserModel { username = _userInfo.Username, email = email };
            await _email.SendMessage(newUserModel, messageHeader, message);


            HttpContext.Session.SetInt32(_userInfo.UserId.ToString(), code);
            HttpContext.Session.SetString(EMAIL, email);
            _logger.LogInformation($"Code and email was saved in user session {_userInfo.Username}#{_userInfo.UserId}");

            _logger.LogInformation($"Email to {_userInfo.Username}#{_userInfo.UserId} was sended on {email} (3-rd step)");
            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
        }

        [HttpPut("confirm/new")]
        public async Task<IActionResult> ConfirmAndUpdateNewEmail([FromQuery] int code)
        {
            try
            {
                int correctCode = (int)HttpContext.Session.GetInt32(_userInfo.UserId.ToString());
                string? email = HttpContext.Session.GetString(EMAIL);
                _logger.LogInformation($"Code and email were received from user session {_userInfo.Username}#{_userInfo.UserId}. code: {correctCode}, email: {email}");

                if (email is null || !_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!code.Equals(correctCode))
                    return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

                var newUserModel = new UserModel { id = _userInfo.UserId, email = email };
                await _update.Update(newUserModel, null);
                _logger.LogInformation("Email was was updated in db");

                await _tokenService.UpdateJwtToken();
                _logger.LogInformation("jwt with a new claims was updated");

                HttpContext.Session.Remove(_userInfo.UserId.ToString());
                HttpContext.Session.Remove(EMAIL);
                _logger.LogInformation($"User session {_userInfo.Username}#{_userInfo.UserId} has been cleared");

                return StatusCode(201);
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"Non-existent user {_userInfo.Username}#{_userInfo.UserId} was requested to authorized endpoint.\nTrying delete tokens from cookie");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(409, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Error when trying to update jwt.\nTrying delete tokens");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}
