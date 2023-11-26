using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.DB.SQL.Tokens;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL.Tokens;
using webapi.Models;

namespace webapi.Controllers.Account
{
    [Route("api/auth/session")]
    [ApiController]
    public class AuthSessionController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IUpdateToken _updateToken;

        public AuthSessionController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IUpdateToken updateToken)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _updateToken = updateToken;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserModel userModel)
        {
            try
            {
                string email = userModel.email.ToLowerInvariant();

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
                if (user is null)
                    return StatusCode(404);

                bool IsCorrect = _passwordManager.CheckPassword(userModel.password_hash, user.password_hash);
                if (!IsCorrect)
                    return StatusCode(401);

                string refreshToken = _tokenService.GenerateRefreshToken();
                string refreshTokenHash = _tokenService.HashingToken(refreshToken);
                var refreshCookieOptions = _tokenService.SetCookieOptions(TimeSpan.FromDays(90));

                var newUserModel = new UserModel
                {
                    id = user.id,
                    username = user.username,
                    email = user.email,
                    role = user.role,
                };

                var newTokenModel = new TokenModel
                {
                    user_id = user.id,
                    refresh_token = refreshTokenHash,
                    expiry_date = DateTime.UtcNow.AddDays(90)
                };

                string jwtToken = _tokenService.GenerateJwtToken(newUserModel, 20);
                var jwtCookieOptions = _tokenService.SetCookieOptions(TimeSpan.FromMinutes(20));

                await _updateToken.UpdateRefreshToken(newTokenModel, UpdateToken.USER_ID);

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
            catch (ArgumentException)
            {
                return StatusCode(500);
            }
        }

        [HttpPut("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var tokenModel = new TokenModel() { user_id = _userInfo.UserId, refresh_token = "", expiry_date = DateTime.UtcNow.AddYears(-100) };

                await _updateToken.UpdateRefreshToken(tokenModel, UpdateToken.USER_ID);
                _tokenService.DeleteTokens();

                return StatusCode(200);
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ArgumentException)
            {
                return StatusCode(500);
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
