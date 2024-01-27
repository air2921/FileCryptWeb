using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using UAParser;
using webapi.DB;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
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

        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<AuthSessionController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly IUserAgent _userAgent;
        private readonly IEmailSender _emailSender;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly IUpdate<TokenModel> _updateToken;
        private readonly ICreate<NotificationModel> _createNotification;

        public AuthSessionController(
            FileCryptDbContext dbContext,
            ILogger<AuthSessionController> logger,
            IUserInfo userInfo,
            IUserAgent userAgent,
            IEmailSender emailSender,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IGenerateSixDigitCode generateCode,
            IUpdate<TokenModel> updateToken,
            ICreate<NotificationModel> createNotification)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userInfo = userInfo;
            _userAgent = userAgent;
            _emailSender = emailSender;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _generateCode = generateCode;
            _updateToken = updateToken;
            _createNotification = createNotification;
        }

        #region Factical login endpoints and helped method

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthDTO userDTO)
        {
            try
            {
                var email = userDTO.email.ToLowerInvariant();
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
                if (user is null)
                    return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

                if (user.is_blocked == true)
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

                return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("verify/2fa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFA([FromQuery] int code)
        {
            string? correctCode = HttpContext.Session.GetString(CODE);
            string? id = HttpContext.Session.GetString(ID);

            if (correctCode is null || id is null)
                return StatusCode(500);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == int.Parse(id));
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

                await _updateToken.Update(new TokenModel
                {
                    user_id = user.id,
                    refresh_token = _tokenService.HashingToken(refreshToken),
                    expiry_date = DateTime.UtcNow + Constants.RefreshExpiry
                }, true);
                _logger.LogInformation("Refresh token was updated in db");

                await _createNotification.Create(new NotificationModel
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

                _logger.LogInformation("Jwt and refresh was sended to client");

                return StatusCode(201);
            }
            catch (TokenException ex)
            {
                return StatusCode(404, new { message = ex.Message });
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
                var tokenModel = new TokenModel() { user_id = _userInfo.UserId, refresh_token = null, expiry_date = DateTime.UtcNow.AddYears(-100) };

                await _updateToken.Update(tokenModel, true);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} refresh token was revoked");

                _tokenService.DeleteTokens();
                _logger.LogInformation("Tokens was revoked from client");

                return StatusCode(200);
            }
            catch (UserException ex)
            {
                _logger.LogCritical("When trying to update the data, the user was deleted");

                _tokenService.DeleteTokens();
                _logger.LogDebug("Tokens was deleted");

                return StatusCode(404, new { message = ex.Message });
            }
            finally
            {
                await _redisCache.DeleteCache(_redisKeys.PrivateKey);
                await _redisCache.DeleteCache(_redisKeys.InternalKey);
                await _redisCache.DeleteCache(_redisKeys.ReceivedKey);
                HttpContext.Session.Clear();

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
