using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    public class AuthSessionController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IUpdate<TokenModel> _update;

        public AuthSessionController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IUpdate<TokenModel> update)
        {
            _dbContext = dbContext;
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

                string refreshToken = _tokenService.GenerateRefreshToken();
                var refreshCookieOptions = _tokenService.SetCookieOptions(TimeSpan.FromDays(90));

                var newUserModel = new UserModel
                {
                    id = user.id,
                    username = user.username,
                    email = user.email,
                    role = user.role,
                };

                var tokenModel = new TokenModel
                {
                    user_id = user.id,
                    refresh_token = _tokenService.HashingToken(refreshToken),
                    expiry_date = DateTime.UtcNow.AddDays(90)
                };

                string jwtToken = _tokenService.GenerateJwtToken(newUserModel, 20);
                var jwtCookieOptions = _tokenService.SetCookieOptions(TimeSpan.FromMinutes(20));

                await _update.Update(tokenModel, true);

                Response.Cookies.Append("JwtToken", jwtToken, jwtCookieOptions);
                Response.Cookies.Append("RefreshToken", refreshToken, refreshCookieOptions);

                return StatusCode(200,
                    new
                    {
                        jwt = new
                        {
                            token = jwtToken,
                            expiry = DateTime.UtcNow.AddMinutes(20)
                        },
                        refresh = new
                        {
                            token = refreshToken,
                            expiry = DateTime.UtcNow.AddDays(90)
                        }
                    });
            }
            catch (UserException)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }
        }

        [HttpPut("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var tokenModel = new TokenModel() { user_id = _userInfo.UserId, refresh_token = "", expiry_date = DateTime.UtcNow.AddYears(-100) };

                await _update.Update(tokenModel, true);
                _tokenService.DeleteTokens();

                return StatusCode(200);
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
            finally
            {
                await _redisCache.DeleteCache(_redisKeys.PrivateKey);
                await _redisCache.DeleteCache(_redisKeys.PersonalInternalKey);
                await _redisCache.DeleteCache(_redisKeys.ReceivedInternalKey);
            }
        }
    }
}
