using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [ValidateAntiForgeryToken]
    public class AuthSessionController : ControllerBase
    {
        private const string CODE = "Code";
        private const string ID = "Id";
        private const string EMAIL = "Email";
        private const string ROLE = "ROLE";
        private const string USERNAME = "Username";

        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<AuthSessionController> _logger;
        private readonly IUserInfo _userInfo;
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
        public async Task<IActionResult> Login(UserModel userModel)
        {
            try
            {
                if (userModel.email is null || userModel.password is null)
                    return StatusCode(422, new { message = AccountErrorMessage.InvalidUserData });

                var email = userModel.email.ToLowerInvariant();
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
                if (user is null)
                    return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

                bool IsCorrect = _passwordManager.CheckPassword(userModel.password, user.password!);
                if (!IsCorrect)
                    return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

                var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());

                if ((bool)!user.is_2fa_enabled)
                    return await FactLogin(clientInfo, user);

                int code = _generateCode.GenerateSixDigitCode();
                var emailDto = new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.Verify2FaHeader,
                    message = EmailMessage.Verify2FaBody + code
                };

                await _emailSender.SendMessage(emailDto);

                HttpContext.Session.SetString(ID, user.id.ToString());
                HttpContext.Session.SetString(EMAIL, email);
                HttpContext.Session.SetString(USERNAME, user.username);
                HttpContext.Session.SetString(ROLE, user.role);
                HttpContext.Session.SetString(CODE, code.ToString());

                return StatusCode(307, new { message = AccountSuccessMessage.EmailSended });
            }
            catch (UserException)
            {
                _logger.LogCritical("When trying to update the data, the user was deleted");
                _tokenService.DeleteTokens();
                _logger.LogDebug("Tokens was deleted");
                return StatusCode(404);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("verify/2fa")]
        public async Task<IActionResult> VerifyTwoFA([FromQuery] int code)
        {
            int correctCode = int.Parse(HttpContext.Session.GetString(CODE));
            int userId = int.Parse(HttpContext.Session.GetString(ID));
            string? email = HttpContext.Session.GetString(EMAIL);
            string? username = HttpContext.Session.GetString(USERNAME);
            string? role = HttpContext.Session.GetString(ROLE);

            if (email is null || username is null || role is null)
                return StatusCode(422, new { message = AccountErrorMessage.NullUserData });

            if (!code.Equals(correctCode))
                return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

            var clientInfo = Parser.GetDefault().Parse(HttpContext.Request.Headers["User-Agent"].ToString());
            var userModel = new UserModel
            {
                id = userId,
                username = username,
                email = email,
                role = role
            };

            return await FactLogin(clientInfo, userModel);
        }

        private async Task<IActionResult> FactLogin(ClientInfo clientInfo, UserModel user)
        {
            try
            {
                var userModel = new UserModel
                {
                    id = user.id,
                    username = user.username,
                    email = user.email,
                    role = user.role,
                };

                var browser = clientInfo.UA.Family;
                var browserVersion = clientInfo.UA.Major + "." + clientInfo.UA.Minor;
                var os = clientInfo.OS.Family;

                var notificationModel = new NotificationModel
                {
                    message_header = "Someone has accessed your account",
                    message = $"Someone signed in to your account {user.username}#{user.id} at {DateTime.UtcNow} from {browser} {browserVersion} on OS {os}",
                    priority = Priority.Security.ToString(),
                    send_time = DateTime.UtcNow,
                    is_checked = false,
                    receiver_id = user.id
                };

                string refreshToken = _tokenService.GenerateRefreshToken();

                var tokenModel = new TokenModel
                {
                    user_id = user.id,
                    refresh_token = _tokenService.HashingToken(refreshToken),
                    expiry_date = DateTime.UtcNow + Constants.RefreshExpiry
                };

                await _updateToken.Update(tokenModel, true);
                _logger.LogInformation("Refresh token was updated in db");

                await _createNotification.Create(notificationModel);
                _logger.LogInformation("Created notification about logged in account");

                Response.Cookies.Append(Constants.JWT_COOKIE_KEY, _tokenService.GenerateJwtToken(userModel, Constants.JwtExpiry), _tokenService.SetCookieOptions(Constants.JwtExpiry));
                Response.Cookies.Append(Constants.REFRESH_COOKIE_KEY, refreshToken, _tokenService.SetCookieOptions(Constants.RefreshExpiry));
                _logger.LogInformation("Jwt and refresh was sended to client");

                return StatusCode(200);
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
