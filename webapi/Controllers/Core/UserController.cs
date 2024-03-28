using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Core;
using webapi.Services.Core.Data_Handlers;

namespace webapi.Controllers.Core
{
    [Route("api/core/users")]
    [ApiController]
    [Authorize]
    public class UserController(
        IUserHelpers helpers,
        ICacheHandler<UserModel> cache,
        IRepository<UserModel> userRepository,
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
                var user = await cache.CacheAndGet(new UserObject(cacheKey, userId, own));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { user });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (FormatException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpGet("range")]
        public async Task<IActionResult> FindUserRange(string username)
        {
            try
            {
                var cacheKey = $"User_List_{username}";
                var users = await cache.CacheAndGetRange(new UserRangeObject(cacheKey, username));

                return StatusCode(200, new { users });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (FormatException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }
    }
}
