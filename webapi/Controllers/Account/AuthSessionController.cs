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
using webapi.Services;

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
        private readonly IGenerateSixDigitCode _generateCode;

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
            IGenerateSixDigitCode generateCode)
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
            _generateCode = generateCode;
        }

        #region Factical login endpoints and helped method

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthDTO userDTO)
        {
            var email = userDTO.email.ToLowerInvariant();

            var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
            if (user is null)
                return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

            if (user.is_blocked)
                return StatusCode(403, new { message = AccountErrorMessage.UserBlocked });

            bool IsCorrect = _passwordManager.CheckPassword(userDTO.password, user.password!);
            if (!IsCorrect)
                return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

            var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());

            if (!user.is_2fa_enabled)
                return await CreateTokens(clientInfo, user);

            int code = _generateCode.GenerateSixDigitCode();

            await _emailSender.SendMessage(new EmailDto
            {
                username = user.username,
                email = user.email,
                subject = EmailMessage.Verify2FaHeader,
                message = EmailMessage.Verify2FaBody + code
            });

            HttpContext.Session.SetString(ID, user.id.ToString());
            HttpContext.Session.SetString(CODE, _passwordManager.HashingPassword(code.ToString()));

            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended, confirm = true });
        }

        [HttpPost("verify/2fa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFA([FromQuery] int code)
        {
            string? correctCode = HttpContext.Session.GetString(CODE);
            string? id = HttpContext.Session.GetString(ID);

            if (correctCode is null || id is null)
                return StatusCode(500);

            var user = await _userRepository.GetById(int.Parse(id));
            if (user is null)
                return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

            bool IsCorrect = _passwordManager.CheckPassword(code.ToString(), correctCode);
            if (!IsCorrect)
                return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

            var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());

            return await CreateTokens(clientInfo, user);
        }

        private async Task<IActionResult> CreateTokens(ClientInfo clientInfo, UserModel user)
        {
            try
            {
                var ua = _userAgent.GetBrowserData(clientInfo);

                string refreshToken = _tokenService.GenerateRefreshToken();

                var tokenModel = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(user.id)));
                tokenModel.refresh_token = _tokenService.HashingToken(refreshToken);
                tokenModel.expiry_date = DateTime.UtcNow + Constants.RefreshExpiry;

                await _tokenRepository.Update(tokenModel);

                _logger.LogInformation("Refresh token was updated in db");

                await _notificationRepository.Add(new NotificationModel
                {
                    message_header = "Someone has accessed your account",
                    message = $"Someone signed in to your account {user.username}#{user.id} at {DateTime.UtcNow} from {ua.Browser} {ua.Version} on OS {ua.OS}",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = user.id
                });
                _logger.LogInformation("Created notification about logged in account");

                Response.Cookies.Append(Constants.JWT_COOKIE_KEY, _tokenService.GenerateJwtToken(user, Constants.JwtExpiry), _tokenService.SetCookieOptions(Constants.JwtExpiry));
                Response.Cookies.Append(Constants.REFRESH_COOKIE_KEY, refreshToken, _tokenService.SetCookieOptions(Constants.RefreshExpiry));

                var cookieOptions = new CookieOptions
                {
                    MaxAge = Constants.JwtExpiry,
                    Secure = true,
                    HttpOnly = false,
                    SameSite = SameSiteMode.None,
                    IsEssential = false
                };

                Response.Cookies.Append(Constants.IS_AUTHORIZED, true.ToString(), cookieOptions);
                Response.Cookies.Append(Constants.USER_ID_COOKIE_KEY, user.id.ToString(), cookieOptions);
                Response.Cookies.Append(Constants.USERNAME_COOKIE_KEY, user.username, cookieOptions);
                Response.Cookies.Append(Constants.ROLE_COOKIE_KEY, user.role, cookieOptions);

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
            finally
            {
                await _redisCache.DeleteCache(_redisKeys.PrivateKey);
                await _redisCache.DeleteCache(_redisKeys.InternalKey);
                await _redisCache.DeleteCache(_redisKeys.ReceivedKey);

                _logger.LogInformation("Encryption keys was deleted from cache");
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
