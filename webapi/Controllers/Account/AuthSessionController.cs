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

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    public class AuthSessionController : ControllerBase
    {
        private const string CODE = "Code";
        private const string ID = "Id";

        #region fields and costructor

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<AuthSessionController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly IEmailSender _emailSender;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IGenerate _generate;

        public AuthSessionController(
            IRepository<UserModel> userRepository,
            IRepository<NotificationModel> notificationRepository,
            IRepository<TokenModel> tokenRepository,
            FileCryptDbContext dbContext,
            ILogger<AuthSessionController> logger,
            IUserInfo userInfo,
            IEmailSender emailSender,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IGenerate generate)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _tokenRepository = tokenRepository;
            _dbContext = dbContext;
            _logger = logger;
            _userInfo = userInfo;
            _emailSender = emailSender;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _generate = generate;
        }

        #endregion

        #region Factical login endpoints and helped method

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
                    return await CreateTokens(user);

                int code = _generate.GenerateSixDigitCode();
                await SendMessage(user.username, user.email, code);
                SetSession(HttpContext, user.id, code);

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
        public async Task<IActionResult> VerifyTwoFA([FromQuery] int code)
        {
            try
            {
                var session = GetSession(HttpContext);

                bool IsCorrect = _passwordManager.CheckPassword(code.ToString(), session.Code);
                if (!IsCorrect)
                    return StatusCode(422, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(session.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return await CreateTokens(user);
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Helper]
        [NonAction]
        private async Task<IActionResult> CreateTokens(UserModel user)
        {
            try
            {
                string refreshToken = _tokenService.GenerateRefreshToken();
                var tokenModel = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(user.id)));
                await DbTransaction(tokenModel, user, refreshToken);
                CookieAppend(user, HttpContext, refreshToken);

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
            finally
            {
                HttpContext.Session.Clear();
            }
        }

        [Helper]
        private async Task DbTransaction(TokenModel tokenModel, UserModel user, string refreshToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                tokenModel.refresh_token = _tokenService.HashingToken(refreshToken);
                tokenModel.expiry_date = DateTime.UtcNow + ImmutableData.RefreshExpiry;

                await _tokenRepository.Update(tokenModel);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone has accessed your account",
                    message = $"Someone signed in to your account {user.username}#{user.id} at {DateTime.UtcNow}.",
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
        private async Task SendMessage(string username, string email, int code)
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
        private void SetSession(HttpContext context, int userId, int code)
        {
            context.Session.SetString(ID, userId.ToString());
            context.Session.SetString(CODE, _passwordManager.HashingPassword(code.ToString()));
        }

        [Helper]
        private SessionObject GetSession(HttpContext context)
        {
            string? correctCode = HttpContext.Session.GetString(CODE);
            string? id = HttpContext.Session.GetString(ID);

            if (correctCode is null || id is null)
                throw new ArgumentNullException(Message.ERROR);

            return new SessionObject
            {
                UserId = int.Parse(id),
                Code = correctCode,
            };
        }

        [AuxiliaryObject]
        private class SessionObject
        {
            public int UserId { get; set; }
            public string Code { get; set; }
        }

        #endregion

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
                await RevokeToken();
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
            finally
            {
                HttpContext.Session.Clear();
                await _redisCache.DeleteCache(_redisKeys.PrivateKey);
                await _redisCache.DeleteCache(_redisKeys.InternalKey);
                await _redisCache.DeleteCache(_redisKeys.ReceivedKey);
            }
        }

        [Helper]
        private async Task RevokeToken()
        {
            try
            {
                var tokenModel = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(_userInfo.UserId)));
                tokenModel.refresh_token = Guid.NewGuid().ToString();
                tokenModel.expiry_date = DateTime.UtcNow.AddYears(-100);

                await _tokenRepository.Update(tokenModel);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
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
}
