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
    [Route("api/account/edit/email")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        #region fields and constuctor

        private readonly string EMAIL;
        private readonly string OLD_EMAIL_CODE;
        private readonly string NEW_EMAIL_CODE;

        private readonly IApiEmailService _emailService;
        private readonly IRepository<UserModel> _userRepository;
        private readonly ILogger<EmailController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerate _generate;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;

        public EmailController(
            IApiEmailService emailService,
            IRepository<UserModel> userRepository,
            ILogger<EmailController> logger,
            IPasswordManager passwordManager,
            IGenerate generate,
            ITokenService tokenService,
            IUserInfo userInfo,
            IValidation validation)
        {
            _emailService = emailService;
            _userRepository = userRepository;
            _logger = logger;
            _passwordManager = passwordManager;
            _generate = generate;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _validation = validation;

            EMAIL = $"EmailController_Email#{_userInfo.UserId}";
            OLD_EMAIL_CODE = $"EmailController_ConfirmationCode_OldEmail#{_userInfo.UserId}";
            NEW_EMAIL_CODE = $"EmailController_ConfirmationCode_NewEmail#{_userInfo.UserId}";
        }

        #endregion

        [HttpPost("start")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> StartEmailChangeProcess([FromQuery] string password)
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
                await _emailService.SendMessage(_userInfo.Username, _userInfo.Email, EmailMessage.ConfirmOldEmailHeader, EmailMessage.ConfirmOldEmailBody + code);

                await _emailService.SetData(OLD_EMAIL_CODE, code);

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

        [HttpPost("confirm/old")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> ConfirmOldEmail([FromQuery] string email, [FromQuery] int code)
        {
            try
            {
                int correctCode = await _emailService.GetCode(OLD_EMAIL_CODE);
                email = email.ToLowerInvariant();

                if (!_validation.IsSixDigit(correctCode) || !code.Equals(correctCode))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
                if (user is not null)
                    return StatusCode(409, new { message = Message.CONFLICT });

                int confirmationCode = _generate.GenerateSixDigitCode();
                await _emailService.SendMessage(_userInfo.Username, email, EmailMessage.ConfirmNewEmailHeader, EmailMessage.ConfirmNewEmailBody + confirmationCode);

                await _emailService.SetData(NEW_EMAIL_CODE, confirmationCode);
                await _emailService.SetData(EMAIL, email);

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

        [HttpPut("confirm/new")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        [ProducesResponseType(typeof(object), 206)]
        public async Task<IActionResult> ConfirmAndUpdateNewEmail([FromQuery] int code)
        {
            try
            {
                int correctCode = await _emailService.GetCode(NEW_EMAIL_CODE);
                string? email = await _emailService.GetString(EMAIL);

                if (email is null || !_validation.IsSixDigit(correctCode) || !code.Equals(correctCode))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _emailService.DbTransaction(user, email);
                await _tokenService.UpdateJwtToken();
                await _emailService.ClearData(_userInfo.UserId);

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
                _tokenService.DeleteTokens();
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }

    public interface IApiEmailService
    {
        public Task DbTransaction(UserModel user, string email);
        public Task SendMessage(string username, string email, string header, string body);
        public Task ClearData(int userId);
        public Task SetData(string key, object data);
        public Task<int> GetCode(string key);
        public Task<string> GetString(string key);
    }

    public class EmailService : IApiEmailService
    {
        #region fields and constructor

        private readonly FileCryptDbContext _dbContext;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IUserInfo _userInfo;
        private readonly IEmailSender _emailSender;
        private readonly IRedisCache _redisCache;

        public EmailService(
            FileCryptDbContext dbContext,
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            IUserInfo userInfo,
            IEmailSender emailSender,
            IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _userInfo = userInfo;
            _emailSender = emailSender;
            _redisCache = redisCache;
        }

        #endregion

        [Helper]
        public async Task DbTransaction(UserModel user, string email)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                user.email = email;
                await _userRepository.Update(user);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_EMAIL_CHANGED_HEADER,
                    message = NotificationMessage.AUTH_EMAIL_CHANGED_BODY,
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
        public async Task SendMessage(string username, string email, string header, string body)
        {
            try
            {
                await _emailSender.SendMessage(new EmailDto
                {
                    username = username,
                    email = email,
                    subject = header,
                    message = body
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
            await _redisCache.DeleteCache($"EmailController_Email#{userId}");
            await _redisCache.DeleteCache($"EmailController_ConfirmationCode_OldEmail#{userId}");
            await _redisCache.DeleteCache($"EmailController_ConfirmationCode_NewEmail#{userId}");

            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{userId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{userId}");
        }

        [Helper]
        public async Task<string> GetString(string key)
        {
            var data = await _redisCache.GetCachedData(key);
            if (data is not null)
                return JsonConvert.DeserializeObject<string>(data);
            else
                return null;
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