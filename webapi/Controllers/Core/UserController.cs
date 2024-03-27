using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Core;

namespace webapi.Controllers.Core
{
    [Route("api/core/users")]
    [ApiController]
    [Authorize]
    public class UserController(
        IUserHelpers helpers,
        IRepository<UserModel> userRepository,
        IRedisCache redisCache,
        IUserInfo userInfo,
        ITokenService tokenService) : ControllerBase
    {
        [HttpGet("{userId}/{username}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId, [FromRoute] string username)
        {
            try
            {
                var user = await helpers.GetUserAndKeys(userId);
                var files = await helpers.GetFiles(userId);
                var offers = await helpers.GetOffers(userId);

                return StatusCode(200, new { user = user.user, isOwner = user.isOwner, keys = user.keys, files, offers });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                await userRepository.Delete(userInfo.UserId);
                tokenService.DeleteTokens();
                HttpContext.Session.Clear();

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("find")]
        public async Task<IActionResult> FindUser([FromQuery] int userId, [FromQuery] bool own)
        {
            try
            {
                if (own)
                    userId = userInfo.UserId;

                var cacheKey = $"{ImmutableData.USER_DATA_PREFIX}{userId}";
                var cache = await redisCache.GetCachedData(cacheKey);
                var user = new UserModel();
                
                if (cache is null)
                {
                    user = await userRepository.GetById(userId);
                    if (user is null)
                        return StatusCode(404, new { message = Message.NOT_FOUND });
                    user.password = string.Empty;

                    await redisCache.CacheData(cacheKey, user, TimeSpan.FromMinutes(5));

                    return StatusCode(200, new { user });
                }

                user = JsonConvert.DeserializeObject<UserModel>(cache);
                user.email = user.id.Equals(userInfo.UserId) ? user.email : string.Empty;

                return StatusCode(200, new { user });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> FindUserRange(string username)
        {
            try
            {
                var cacheKey = $"User_List_{username}";
                var cache = await redisCache.GetCachedData(cacheKey);
                if (cache is not null)
                    return StatusCode(200, new { users = JsonConvert.DeserializeObject<IEnumerable<UserModel>>(cache) });

                var users = (List<UserModel>)await userRepository.GetAll(query => query.Where(u => u.username.Equals(username)));
                foreach (var user in users)
                {
                    user.password = string.Empty;
                    user.email = string.Empty;
                }

                await redisCache.CacheData(cacheKey, users, TimeSpan.FromMinutes(5));
                return StatusCode(200, new { users });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
