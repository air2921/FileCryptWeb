using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/2fa")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class _2FaController : ControllerBase
    {
        private const string CODE = "2FaCode";

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRedisCache _redisCache;
        private readonly IPasswordManager _passwordManager;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;
        private readonly ILogger<_2FaController> _logger;
        private readonly IGenerate _generate;
        private readonly IEmailSender _emailSender;
        private readonly IValidation _validation;

        public _2FaController(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            IRedisCache redisCache,
            IPasswordManager passwordManager,
            IUserInfo userInfo,
            ITokenService tokenService,
            ILogger<_2FaController> logger,
            IGenerate generate,
            IEmailSender emailSender,
            IValidation validation)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _redisCache = redisCache;
            _passwordManager = passwordManager;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _logger = logger;
            _generate = generate;
            _emailSender = emailSender;
            _validation = validation;
        }

        [HttpPost("start")]
        public async Task<IActionResult> SendVerificationCode([FromQuery] string password)
        {
            try
            {
                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404);

                bool IsCorrect = _passwordManager.CheckPassword(password, user.password);
                if (!IsCorrect)
                    return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

                int code = _generate.GenerateSixDigitCode();

                await _emailSender.SendMessage(new EmailDto
                {
                    username = _userInfo.Username,
                    email = _userInfo.Email,
                    subject = EmailMessage.Change2FaHeader,
                    message = EmailMessage.Change2FaBody + code
                });

                HttpContext.Session.SetString(CODE, code.ToString());

                return StatusCode(200);
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

        [HttpPut("confirm/{enable}")]
        public async Task<IActionResult> Update2FaState([FromQuery] int code, [FromRoute] bool enable)
        {
            try
            {
                int correctCode = int.TryParse(HttpContext.Session.GetString(CODE), out var parsedValue) ? parsedValue : 0;

                if (!_validation.IsSixDigit(correctCode))
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

                if (user.is_2fa_enabled == enable)
                    return StatusCode(409);

                user.is_2fa_enabled = enable;
                await _userRepository.Update(user);

                string message = enable
                    ? "Your two-factor authentication status has been successfully updated! Your account is now even more secure. Thank you for prioritizing security with us."
                    : "Your two-factor authentication has been disabled. Please ensure that you take additional precautions to secure your account. If this change was not authorized, contact our support team immediately. Thank you for staying vigilant about your account security.";

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = $"Someone changed changed your 2FA status",
                    message = message,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = _userInfo.UserId
                });

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{_userInfo.UserId}");
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{_userInfo.UserId}");

                return StatusCode(200);
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
        }
    }
}
