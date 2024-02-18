using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB;
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

        #region fields and constructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly FileCryptDbContext _dbContext;
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
            FileCryptDbContext dbContext,
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
            _dbContext = dbContext;
            _redisCache = redisCache;
            _passwordManager = passwordManager;
            _userInfo = userInfo;
            _tokenService = tokenService;
            _logger = logger;
            _generate = generate;
            _emailSender = emailSender;
            _validation = validation;
        }

        #endregion

        [HttpPost("start")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SendVerificationCode([FromQuery] string password)
        {
            try
            {
                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                bool IsCorrect = _passwordManager.CheckPassword(password, user.password);
                if (!IsCorrect)
                    return StatusCode(401, new { message = Message.INCORRECT });

                int code = _generate.GenerateSixDigitCode();
                await SendMessage(_userInfo.Username, _userInfo.Email, code);

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
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Update2FaState([FromQuery] int code, [FromRoute] bool enable)
        {
            try
            {
                int correctCode = int.TryParse(HttpContext.Session.GetString(CODE), out var parsedValue) ? parsedValue : 0;
                if (!_validation.IsSixDigit(correctCode) || !code.Equals(correctCode))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await DbTransaction(user, enable);
                await ClearData(HttpContext);

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

        [Helper]
        private async Task DbTransaction(UserModel user, bool enable)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (user.is_2fa_enabled == enable)
                    throw new EntityNotUpdatedException(Message.CONFLICT);
                user.is_2fa_enabled = enable;
                await _userRepository.Update(user);

                string message = enable
                    ? "Your two-factor authentication status has been successfully updated! Your account is now even more secure. Thank you for prioritizing security with us."
                    : "Your two-factor authentication has been disabled. Please ensure that you take additional precautions to secure your account. If this change was not authorized, contact our support team immediately. Thank you for staying vigilant about your account security.";

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = $"2FA Status was updated",
                    message = message,
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = _userInfo.UserId
                });

                await transaction.CommitAsync();
            }
            catch (EntityNotCreatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (EntityNotUpdatedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (OperationCanceledException)
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
                await _emailSender.SendMessage(new EmailDto
                {
                    username = username,
                    email = email,
                    subject = EmailMessage.Change2FaHeader,
                    message = EmailMessage.Change2FaBody + code
                });
            }
            catch (SmtpClientException)
            {
                throw;
            }
        }

        [Helper]
        private async Task ClearData(HttpContext context)
        {
            context.Session.Remove(CODE);
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{_userInfo.UserId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{_userInfo.UserId}");
        }
    }
}
