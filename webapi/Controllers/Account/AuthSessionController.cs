using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
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
        private readonly FileCryptDbContext _dbContext;
        private readonly ILogger<AuthSessionController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IUpdate<TokenModel> _update;

        public AuthSessionController(
            FileCryptDbContext dbContext,
            ILogger<AuthSessionController> logger,
            IUserInfo userInfo,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IUpdate<TokenModel> update)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userInfo = userInfo;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _update = update;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserModel userModel)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == userModel.email.ToLowerInvariant());
                if (user is null)
                    return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });

                bool IsCorrect = _passwordManager.CheckPassword(userModel.password_hash, user.password_hash);
                if (!IsCorrect)
                    return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

                var newUserModel = new UserModel
                {
                    id = user.id,
                    username = user.username,
                    email = user.email,
                    role = user.role,
                };

                string refreshToken = _tokenService.GenerateRefreshToken();

                var tokenModel = new TokenModel
                {
                    user_id = user.id,
                    refresh_token = _tokenService.HashingToken(refreshToken),
                    expiry_date = DateTime.UtcNow.AddDays(Constants.REFRESH_EXPIRY)
                };

                await _update.Update(tokenModel, true);
                _logger.LogInformation("Refresh token was updated in db");

                Response.Cookies.Append(Constants.JWT_COOKIE_KEY, _tokenService.GenerateJwtToken(newUserModel, Constants.JWT_EXPIRY), _tokenService.SetCookieOptions(TimeSpan.FromMinutes(Constants.JWT_EXPIRY)));
                Response.Cookies.Append(Constants.REFRESH_COOKIE_KEY, refreshToken, _tokenService.SetCookieOptions(TimeSpan.FromDays(Constants.REFRESH_EXPIRY)));
                _logger.LogInformation("Jwt and refresh was sended to client");

                return StatusCode(200);
            }
            catch (UserException)
            {
                _logger.LogCritical("When trying to update the data, the user was deleted");
                _tokenService.DeleteTokens();
                _logger.LogDebug("Tokens was deleted");
                return StatusCode(404);
            }
        }

        [HttpPut("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var tokenModel = new TokenModel() { user_id = _userInfo.UserId, refresh_token = null, expiry_date = DateTime.UtcNow.AddYears(-100) };

                await _update.Update(tokenModel, true);
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
                await _redisCache.DeleteCache(_redisKeys.PersonalInternalKey);
                await _redisCache.DeleteCache(_redisKeys.ReceivedInternalKey);

                _logger.LogInformation("Encryption keys was deleted from cache");
            }
        }
    }
}
