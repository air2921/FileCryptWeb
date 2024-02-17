using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAParser;
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

        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly ILogger<AuthSessionController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly IUserAgent _userAgent;
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
            ILogger<AuthSessionController> logger,
            IUserInfo userInfo,
            IUserAgent userAgent,
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
            _logger = logger;
            _userInfo = userInfo;
            _userAgent = userAgent;
            _emailSender = emailSender;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _generate = generate;
        }

        #region Factical login endpoints and helped method

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthDTO userDTO)
        {
            try
            {
                var email = userDTO.email.ToLowerInvariant();

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (user.is_blocked)
                    return StatusCode(403, new { message = Message.BLOCKED });

                bool IsCorrect = _passwordManager.CheckPassword(userDTO.password, user.password!);
                if (!IsCorrect)
                    return StatusCode(401, new { message = Message.INCORRECT });

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());

                if (!user.is_2fa_enabled)
                    return await CreateTokens(clientInfo, user);

                int code = _generate.GenerateSixDigitCode();

                await _emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.Verify2FaHeader,
                    message = EmailMessage.Verify2FaBody + code
                });

                HttpContext.Session.SetString(ID, user.id.ToString());
                HttpContext.Session.SetString(CODE, _passwordManager.HashingPassword(code.ToString()));

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFA([FromQuery] int code)
        {
            try
            {
                string? correctCode = HttpContext.Session.GetString(CODE);
                string? id = HttpContext.Session.GetString(ID);

                if (correctCode is null || id is null)
                    return StatusCode(500);

                bool IsCorrect = _passwordManager.CheckPassword(code.ToString(), correctCode);
                if (!IsCorrect)
                    return StatusCode(422, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(int.Parse(id));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());

                return await CreateTokens(clientInfo, user);
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private async Task<IActionResult> CreateTokens(ClientInfo clientInfo, UserModel user)
        {
            try
            {
                var ua = _userAgent.GetBrowserData(clientInfo);

                string refreshToken = _tokenService.GenerateRefreshToken();

                var tokenModel = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(user.id)));
                tokenModel.refresh_token = _tokenService.HashingToken(refreshToken);
                tokenModel.expiry_date = DateTime.UtcNow + ImmutableData.RefreshExpiry;

                await _tokenRepository.Update(tokenModel);

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone has accessed your account",
                    message = $"Someone signed in to your account {user.username}#{user.id} at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    user_id = user.id
                });

                Response.Cookies.Append(ImmutableData.JWT_COOKIE_KEY, _tokenService.GenerateJwtToken(user, ImmutableData.JwtExpiry), _tokenService.SetCookieOptions(ImmutableData.JwtExpiry));
                Response.Cookies.Append(ImmutableData.REFRESH_COOKIE_KEY, refreshToken, _tokenService.SetCookieOptions(ImmutableData.RefreshExpiry));

                var cookieOptions = new CookieOptions
                {
                    MaxAge = ImmutableData.JwtExpiry,
                    Secure = true,
                    HttpOnly = false,
                    SameSite = SameSiteMode.None,
                    IsEssential = false
                };

                Response.Cookies.Append(ImmutableData.IS_AUTHORIZED, true.ToString(), cookieOptions);
                Response.Cookies.Append(ImmutableData.USER_ID_COOKIE_KEY, user.id.ToString(), cookieOptions);
                Response.Cookies.Append(ImmutableData.USERNAME_COOKIE_KEY, user.username, cookieOptions);
                Response.Cookies.Append(ImmutableData.ROLE_COOKIE_KEY, user.role, cookieOptions);

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

        #endregion

        [HttpPut("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var tokenModel = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(_userInfo.UserId)));
                tokenModel.refresh_token = Guid.NewGuid().ToString();
                tokenModel.expiry_date = DateTime.UtcNow.AddYears(-100);

                await _tokenRepository.Update(tokenModel);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} refresh token was revoked");

                _tokenService.DeleteTokens();
                HttpContext.Session.Clear();
                _logger.LogInformation("Tokens was revoked from client");

                return StatusCode(200);
            }
            catch (EntityNotUpdatedException ex)
            {
                _tokenService.DeleteTokens();
                _logger.LogDebug("Tokens was deleted");

                return StatusCode(404, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            finally
            {
                await _redisCache.DeleteCache(_redisKeys.PrivateKey);
                await _redisCache.DeleteCache(_redisKeys.InternalKey);
                await _redisCache.DeleteCache(_redisKeys.ReceivedKey);
            }
        }

        [HttpGet("check")]
        [Authorize]
        public IActionResult AuthCheck()
        {
            return StatusCode(200);
        }
    }
}
