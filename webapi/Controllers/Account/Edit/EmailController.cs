using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAParser;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
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

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRedisCache _redisCache;
        private readonly IUserAgent _userAgent;
        private readonly IEmailSender _email;
        private readonly ILogger<EmailController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;

        public EmailController(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            IRedisCache redisCache,
            IUserAgent userAgent,
            IEmailSender email,
            ILogger<EmailController> logger,
            IPasswordManager passwordManager,
            IGenerateSixDigitCode generateCode,
            ITokenService tokenService,
            IUserInfo userInfo,
            IValidation validation)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _redisCache = redisCache;
            _userAgent = userAgent;
            _email = email;
            _logger = logger;
            _passwordManager = passwordManager;
            _generateCode = generateCode;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _validation = validation;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartEmailChangeProcess([FromQuery] string password)
        {
            try
            {
                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                {
                    _tokenService.DeleteTokens();
                    _logger.LogWarning("Tokens was deleted");
                    return StatusCode(404);
                }

                bool IsCorrect = _passwordManager.CheckPassword(password, user.password);
                if (!IsCorrect)
                    return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

                int code = _generateCode.GenerateSixDigitCode();

                await _email.SendMessage(new EmailDto
                {
                    username = _userInfo.Username,
                    email = _userInfo.Email,
                    subject = EmailMessage.ConfirmOldEmailHeader,
                    message = EmailMessage.ConfirmOldEmailBody + code
                });

                HttpContext.Session.SetInt32(_userInfo.Email, code);
                _logger.LogInformation($"Code was saved in user session {_userInfo.Username}#{_userInfo.UserId}");
                _logger.LogInformation($"Email to {_userInfo.Username}#{_userInfo.UserId} was sended on {_userInfo.Email} (1-st step)");

                return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
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

        [HttpPost("confirm/old")]
        public async Task<IActionResult> ConfirmOldEmail([FromQuery] string email, [FromQuery] int code)
        {
            try
            {
                int correctCode = int.TryParse(HttpContext.Session.GetString(_userInfo.Email), out var parsedValue) ? parsedValue : 0;
                _logger.LogInformation($"Code were received from user session {_userInfo.Username}#{_userInfo.UserId}. code: {correctCode}");

                if (!_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!code.Equals(correctCode))
                    return StatusCode(401, new { message = AccountErrorMessage.CodeIncorrect });

                _logger.LogInformation($"User {_userInfo.Username}#{_userInfo.UserId} confirmed code (2-nd step)");

                //Here is 2 steps in single endpoint, for best user experience,
                //if this doesn't fit your business logic, you can split that logic into two different endpoints

                email = email.ToLowerInvariant();

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
                if (user is not null)
                    return StatusCode(409, new { message = AccountErrorMessage.UserExists });

                int confirmationCode = _generateCode.GenerateSixDigitCode();

                await _email.SendMessage(new EmailDto
                {
                    username = _userInfo.Username,
                    email = email,
                    subject = EmailMessage.ConfirmNewEmailHeader,
                    message = EmailMessage.ConfirmNewEmailBody + confirmationCode
                });

                HttpContext.Session.SetInt32(_userInfo.UserId.ToString(), confirmationCode);
                HttpContext.Session.SetString(EMAIL, email);
                _logger.LogInformation($"Code and email was saved in user session {_userInfo.Username}#{_userInfo.UserId}");

                _logger.LogInformation($"Email to {_userInfo.Username}#{_userInfo.UserId} was sended on {email} (3-rd step)");
                return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
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

        [HttpPut("confirm/new")]
        public async Task<IActionResult> ConfirmAndUpdateNewEmail([FromQuery] int code)
        {
            try
            {
                int correctCode = int.TryParse(HttpContext.Session.GetString(_userInfo.UserId.ToString()), out var parsedValue) ? parsedValue : 0;
                string? email = HttpContext.Session.GetString(EMAIL);
                _logger.LogInformation($"Code and email were received from user session {_userInfo.Username}#{_userInfo.UserId}. code: {correctCode}, email: {email}");

                if (email is null || !_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!code.Equals(correctCode))
                    return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                {
                    _tokenService.DeleteTokens();
                    _logger.LogWarning("Tokens was deleted");
                    return StatusCode(404);
                }

                user.email = email;
                await _userRepository.Update(user);
                _logger.LogInformation("Email was was updated in db");

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
                var ua = _userAgent.GetBrowserData(clientInfo);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone changed your account email/login",
                    message = $"Someone changed your email at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}." +
                    $"New email: '{email}'",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = _userInfo.UserId
                });

                await _redisCache.DeteteCacheByKeyPattern($"Notifications_{_userInfo.UserId}");

                await _tokenService.UpdateJwtToken();
                _logger.LogInformation("jwt with a new claims was updated");

                HttpContext.Session.Remove(_userInfo.UserId.ToString());
                HttpContext.Session.Remove(EMAIL);
                _logger.LogInformation($"User session {_userInfo.Username}#{_userInfo.UserId} has been cleared");

                await _redisCache.DeteteCacheByKeyPattern($"User_Data_{_userInfo.UserId}");

                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
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