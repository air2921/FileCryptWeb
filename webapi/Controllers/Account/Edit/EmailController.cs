using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UAParser;
using webapi.DB;
using webapi.DTO;
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
    [ValidateAntiForgeryToken]
    public class EmailController : ControllerBase
    {
        private const string EMAIL = "Email";

        private readonly FileCryptDbContext _dbContext;
        private readonly ICreate<NotificationModel> _createNotification;
        private readonly IUpdate<UserModel> _update;
        private readonly IEmailSender _email;
        private readonly ILogger<EmailController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;

        public EmailController(
            FileCryptDbContext dbContext,
            ICreate<NotificationModel> createNotification,
            IUpdate<UserModel> update,
            IEmailSender email,
            ILogger<EmailController> logger,
            IPasswordManager passwordManager,
            IGenerateSixDigitCode generateCode,
            ITokenService tokenService,
            IUserInfo userInfo,
            IValidation validation)
        {
            _dbContext = dbContext;
            _createNotification = createNotification;
            _update = update;
            _email = email;
            _logger = logger;
            _passwordManager = passwordManager;
            _generateCode = generateCode;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _validation = validation;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartEmailChangeProcess([FromBody] UserModel userModel)
        {
            try
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

                var emailDto = new EmailDto
                { 
                    username = _userInfo.Username,
                    email = _userInfo.Email,
                    subject = EmailMessage.ConfirmOldEmailHeader,
                    message = EmailMessage.ConfirmOldEmailBody + code
                };

                await _email.SendMessage(emailDto);

                HttpContext.Session.SetInt32(_userInfo.Email, code);
                _logger.LogInformation($"Code was saved in user session {_userInfo.Username}#{_userInfo.UserId}");

                _logger.LogInformation($"Email to {_userInfo.Username}#{_userInfo.UserId} was sended on {_userInfo.Email} (1-st step)");
                return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("confirm/old")]
        public async Task<IActionResult> ConfirmOldEmail([FromBody] UserModel userModel, [FromQuery] int code)
        {
            try
            {
                int correctCode = (int)HttpContext.Session.GetInt32(_userInfo.Email);
                _logger.LogInformation($"Code were received from user session {_userInfo.Username}#{_userInfo.UserId}. code: {correctCode}");

                if (!_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!code.Equals(correctCode))
                    return StatusCode(401, new { message = AccountErrorMessage.CodeIncorrect });

                _logger.LogInformation($"User {_userInfo.Username}#{_userInfo.UserId} confirmed code (2-nd step)");

                //Here is 2 steps in single endpoint, for best user experience,
                //if this doesn't fit your business logic, you can split that logic into two different endpoints

                string email = userModel.email.ToLowerInvariant();

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
                if (user is not null)
                    return StatusCode(409, new { message = AccountErrorMessage.UserExists });

                int confirmationCode = _generateCode.GenerateSixDigitCode();

                var emailDto = new EmailDto
                {
                    username = _userInfo.Username,
                    email = email,
                    subject = EmailMessage.ConfirmNewEmailHeader,
                    message = EmailMessage.ConfirmNewEmailBody + confirmationCode
                };

                await _email.SendMessage(emailDto);

                HttpContext.Session.SetInt32(_userInfo.UserId.ToString(), confirmationCode);
                HttpContext.Session.SetString(EMAIL, email);
                _logger.LogInformation($"Code and email was saved in user session {_userInfo.Username}#{_userInfo.UserId}");

                _logger.LogInformation($"Email to {_userInfo.Username}#{_userInfo.UserId} was sended on {email} (3-rd step)");
                return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
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

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var browser = clientInfo.UA.Family;
                var browserVersion = clientInfo.UA.Major + "." + clientInfo.UA.Minor;
                var os = clientInfo.OS.Family;

                var notificationModel = new NotificationModel
                {
                    message_header = "Someone changed your account email/login",
                    message = $"Someone changed your email at {DateTime.UtcNow} from {browser} {browserVersion} on OS {os}." +
                    $"New email: '{email}'",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = _userInfo.UserId
                };

                await _createNotification.Create(notificationModel);

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