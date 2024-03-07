using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    public class _2FaController : ControllerBase
    {
        private readonly string CODE;

        #region fields and constructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IApi2FaService _api2FaService;
        private readonly IPasswordManager _passwordManager;
        private readonly IUserInfo _userInfo;
        private readonly IGenerate _generate;
        private readonly IValidation _validation;

        public _2FaController(
            IRepository<UserModel> userRepository,
            IApi2FaService api2FaService,
            IPasswordManager passwordManager,
            IUserInfo userInfo,
            IGenerate generate,
            IValidation validation)
        {
            _userRepository = userRepository;
            _api2FaService = api2FaService;
            _passwordManager = passwordManager;
            _userInfo = userInfo;
            _generate = generate;
            _validation = validation;
            CODE = $"_2FaController_VerificationCode#{_userInfo.UserId}";
        }

        #endregion

        [HttpPost("start")]
        [XSRFProtection]
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
                await _api2FaService.SendMessage(user.username, user.email, code);

                await _api2FaService.SetData(CODE, code);

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
        [XSRFProtection]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Update2FaState([FromQuery] int code, [FromRoute] bool enable)
        {
            try
            {
                int correctCode = await _api2FaService.GetCode(CODE);
                if (!_validation.IsSixDigit(correctCode) || !code.Equals(correctCode))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _api2FaService.DbTransaction(user, enable);
                await _api2FaService.ClearData(_userInfo.UserId);

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

    public interface IApi2FaService
    {
        public Task DbTransaction(UserModel user, bool enable);
        public Task SendMessage(string username, string email, int code);
        public Task ClearData(int userId);
        public Task SetData(string key, object data);
        public Task<int> GetCode(string key);
    }

    public class _2FaService : IApi2FaService
    {
        private const string CODE = "2FaCode";

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IEmailSender _emailSender;

        public _2FaService(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            FileCryptDbContext dbContext,
            IRedisCache redisCache,
            IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _dbContext = dbContext;
            _redisCache = redisCache;
            _emailSender = emailSender;
        }

        [Helper]
        public async Task DbTransaction(UserModel user, bool enable)
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
                    user_id = user.id
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
        }

        [Helper]
        public async Task SendMessage(string username, string email, int code)
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
        public async Task ClearData(int userId)
        {
            await _redisCache.DeleteCache($"_2FaController_VerificationCode#{userId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{userId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{userId}");
        }

        [Helper]
        public async Task SetData(string key, object data)
        {
            await _redisCache.CacheData(key, data, TimeSpan.FromMinutes(10));
        }

        [Helper]
        public async Task<int> GetCode(string key)
        {
            var code = await _redisCache.GetCachedData(key);
            if (code is not null)
                return JsonConvert.DeserializeObject<int>(code);
            else
                return 0;
        }
    }
}
