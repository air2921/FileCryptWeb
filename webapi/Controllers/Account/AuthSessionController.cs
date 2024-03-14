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

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    public class AuthSessionController : ControllerBase
    {
        private readonly string USER_OBJECT = "AuthSessionController_UserObject_Email:";

        #region fields and costructor

        private readonly IApiSessionService _sessionService;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly ILogger<AuthSessionController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IGenerate _generate;

        public AuthSessionController(
            IApiSessionService sessionService,
            IRepository<UserModel> userRepository,
            IRepository<TokenModel> tokenRepository,
            ILogger<AuthSessionController> logger,
            IUserInfo userInfo,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IGenerate generate)
        {
            _sessionService = sessionService;
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _logger = logger;
            _userInfo = userInfo;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _generate = generate;
        }

        #endregion

        [HttpPost("login")]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Login(AuthDTO userDTO)
        {
            try
            {
                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(userDTO.email.ToLowerInvariant())));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (user.is_blocked)
                    return StatusCode(403, new { message = Message.BLOCKED });

                if (!_passwordManager.CheckPassword(userDTO.password, user.password))
                    return StatusCode(401, new { message = Message.INCORRECT });

                if (!user.is_2fa_enabled)
                    return await _sessionService.CreateTokens(user, HttpContext);

                int code = _generate.GenerateSixDigitCode();
                await _sessionService.SendMessage(user.username, user.email, code);
                await _sessionService.SetData($"{USER_OBJECT}{user.email}", new UserContextObject 
                {
                    UserId = user.id,   
                    Code = _passwordManager.HashingPassword(code.ToString())
                });

                return StatusCode(200, new { message = Message.EMAIL_SENT, confirm = true });
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

        [HttpPost("verify/2fa")]
        [XSRFProtection]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> VerifyTwoFA([FromQuery] int code, [FromQuery] string email)
        {
            try
            {
                var userContext = await _sessionService.GetData($"{USER_OBJECT}{email.ToLowerInvariant()}");
                if (userContext is null)
                    return StatusCode(404, new { message = Message.TASK_TIMED_OUT });

                bool IsCorrect = _passwordManager.CheckPassword(code.ToString(), userContext.Code);
                if (!IsCorrect)
                    return StatusCode(422, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(userContext.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return await _sessionService.CreateTokens(user, HttpContext);
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("logout")]
        [Authorize]
        [XSRFProtection]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _sessionService.RevokeToken(HttpContext);
                _tokenService.DeleteTokens();

                return StatusCode(200);
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult AuthCheck()
        {
            return StatusCode(200);
        }
    }

    public interface IApiSessionService
    {
        public Task<IActionResult> CreateTokens(UserModel user, HttpContext context);
        public Task RevokeToken(HttpContext content);
        public Task SendMessage(string username, string email, int code);
        public Task SetData(string key, UserContextObject user);
        public Task<UserContextObject> GetData(string key);
    }

    public class SessionService : ControllerBase, IApiSessionService
    {
        #region fields and constructor

        private readonly FileCryptDbContext _dbContext;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordManager _passwordManager;
        private readonly IRedisCache _redisCache;
        private readonly ITokenService _tokenService;

        public SessionService(
            FileCryptDbContext dbContext,
            IRepository<TokenModel> tokenRepository,
            IRepository<NotificationModel> notificationRepository,
            IEmailSender emailSender,
            IPasswordManager passwordManager,
            IRedisCache redisCache,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenRepository = tokenRepository;
            _notificationRepository = notificationRepository;
            _emailSender = emailSender;
            _passwordManager = passwordManager;
            _redisCache = redisCache;
            _tokenService = tokenService;
        }

        #endregion

        private void CookieAppend(UserModel user, HttpContext context, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                MaxAge = ImmutableData.JwtExpiry,
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                IsEssential = false
            };

            context.Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, _tokenService.GenerateJwtToken(user, ImmutableData.JwtExpiry), _tokenService.SetCookieOptions(ImmutableData.JwtExpiry));
            context.Response.Cookies.Append(ImmutableData.REFRESH_COOKIE_KEY, refreshToken, _tokenService.SetCookieOptions(ImmutableData.RefreshExpiry));
            context.Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, true.ToString(), cookieOptions);
            context.Response.Cookies.Append(ImmutableData.USER_ID_COOKIE_KEY, user.id.ToString(), cookieOptions);
            context.Response.Cookies.Append(ImmutableData.USERNAME_COOKIE_KEY, user.username, cookieOptions);
            context.Response.Cookies.Append(ImmutableData.ROLE_COOKIE_KEY, user.role, cookieOptions);
        }

        [Helper]
        private async Task DbTransaction(UserModel user, string refreshToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _tokenRepository.Add(new TokenModel
                {
                    user_id = user.id,
                    refresh_token = _tokenService.HashingToken(refreshToken),
                    expiry_date = DateTime.UtcNow + ImmutableData.RefreshExpiry
                });

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = NotificationMessage.AUTH_NEW_LOGIN_HEADER,
                    message = NotificationMessage.AUTH_NEW_LOGIN_BODY,
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
        }

        [Helper]
        [NonAction]
        public async Task<IActionResult> CreateTokens(UserModel user, HttpContext context)
        {
            try
            {
                string refreshToken = _tokenService.GenerateRefreshToken();
                await DbTransaction(user, refreshToken);
                CookieAppend(user, context, refreshToken);

                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");

                return StatusCode(200);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Helper]
        public async Task RevokeToken(HttpContext context)
        {
            try
            {
                var refresh = context.Request.Cookies[ImmutableData.REFRESH_COOKIE_KEY];
                if (refresh is null)
                    return;

                var token = await _tokenRepository.DeleteByFilter(query => query
                    .Where(t => t.refresh_token.Equals(_tokenService.HashingToken(refresh))));
            }
            catch (EntityNotDeletedException)
            {
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
                    subject = EmailMessage.Verify2FaHeader,
                    message = EmailMessage.Verify2FaBody + code
                });
            }
            catch (SmtpClientException)
            {
                throw;
            }
        }

        [Helper]
        public async Task SetData(string key, UserContextObject user)
        {
            await _redisCache.CacheData(key, user, TimeSpan.FromMinutes(8));
        }

        [Helper]
        public async Task<UserContextObject> GetData(string key)
        {
            var user = await _redisCache.GetCachedData(key);
            if (user is not null)
                return JsonConvert.DeserializeObject<UserContextObject>(user);
            else
                return null;
        }
    }

    [AuxiliaryObject]
    public class UserContextObject
    {
        public int UserId { get; set; }
        public string Code { get; set; }
    }
}
