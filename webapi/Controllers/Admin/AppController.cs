using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.Helpers;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/service")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class AppController(IUserInfo userInfo, ILogger<AppController> logger, IRedisCache redisCache) : ControllerBase
    {
        [HttpPut("freeze")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> FreezeService([FromQuery] bool flag, [FromBody] TimeSpan? time)
        {
            if (flag && time.HasValue)
            {
                await redisCache.CacheData(ImmutableData.SERVICE_FREEZE_FLAG, flag, time.Value);
                logger.LogCritical($"{userInfo.Username}#{userInfo.UserId} freezed service until {DateTime.UtcNow + time}");

                return StatusCode(200, new { message = $"Service freezed until {DateTime.UtcNow + time}" });
            }

            await redisCache.DeleteCache(ImmutableData.SERVICE_FREEZE_FLAG);
            logger.LogCritical($"{userInfo.Username}#{userInfo.UserId} unfreezed service at {DateTime.UtcNow}");

            return StatusCode(200, new { message = "Service unfreezed" });
        }
    }
}
