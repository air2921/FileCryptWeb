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
    [Route("api/account/edit/email")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class EmailController : ControllerBase
    {
        private const string EMAIL = "Email";

        #region fields and constuctor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IEmailSender _email;
        private readonly ILogger<EmailController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerate _generate;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;

        public EmailController(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            FileCryptDbContext dbContext,
            IRedisCache redisCache,
            IEmailSender email,
            ILogger<EmailController> logger,
            IPasswordManager passwordManager,
            IGenerate generate,
            ITokenService tokenService,
            IUserInfo userInfo,
            IValidation validation)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _dbContext = dbContext;
            _redisCache = redisCache;
            _email = email;
            _logger = logger;
            _passwordManager = passwordManager;
            _generate = generate;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _validation = validation;
        }

        #endregion

        [HttpPost("start")]
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
                await SendMessage(_userInfo.Username, _userInfo.Email, EmailMessage.ConfirmOldEmailHeader, EmailMessage.ConfirmOldEmailBody + code);

                HttpContext.Session.SetInt32(_userInfo.Email, code);

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
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> ConfirmOldEmail([FromQuery] string email, [FromQuery] int code)
        {
            try
            {
                int correctCode = int.TryParse(HttpContext.Session.GetString(_userInfo.Email), out var parsedValue) ? parsedValue : 0;
                email = email.ToLowerInvariant();

                if (!_validation.IsSixDigit(correctCode) || !code.Equals(correctCode))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
                if (user is not null)
                    return StatusCode(409, new { message = Message.CONFLICT });

                int confirmationCode = _generate.GenerateSixDigitCode();
                await SendMessage(_userInfo.Username, email, EmailMessage.ConfirmNewEmailHeader, EmailMessage.ConfirmNewEmailBody + confirmationCode);

                HttpContext.Session.SetInt32(_userInfo.UserId.ToString(), confirmationCode);
                HttpContext.Session.SetString(EMAIL, email);

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
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        [ProducesResponseType(typeof(object), 206)]
        public async Task<IActionResult> ConfirmAndUpdateNewEmail([FromQuery] int code)
        {
            try
            {
                int correctCode = int.TryParse(HttpContext.Session.GetString(_userInfo.UserId.ToString()), out var parsedValue) ? parsedValue : 0;
                string? email = HttpContext.Session.GetString(EMAIL);

                if (email is null || !_validation.IsSixDigit(correctCode) || !code.Equals(correctCode))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await DbTransaction(user, email);
                await _tokenService.UpdateJwtToken();
                await ClearData(HttpContext);

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

        [Helper]
        private async Task DbTransaction(UserModel user, string email)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                user.email = email;
                await _userRepository.Update(user);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone changed your account email/login",
                    message = $"Someone changed your email at {DateTime.UtcNow}." +
                    $"New email: '{email}'",
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
        private async Task SendMessage(string username, string email, string header, string body)
        {
            try
            {
                await _email.SendMessage(new EmailDto
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
        private async Task ClearData(HttpContext context)
        {
            context.Session.Clear();
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{_userInfo.UserId}");
            await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{_userInfo.UserId}");
        }
    }
}