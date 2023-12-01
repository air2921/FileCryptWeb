using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces.Redis;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/service")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class AppController : ControllerBase
    {
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;

        public AppController(IRedisCache redisCache, IRedisKeys redisKeys)
        {
            _redisCache = redisCache;
            _redisKeys = redisKeys;
        }

        [HttpPut("freeze/{freezeFlag}")]
        public async Task<IActionResult> FreezeService([FromRoute] bool freezeFlag, [FromBody] TimeSpan? time)
        {
            if (freezeFlag && time.HasValue)
            {
                await _redisCache.CacheData(_redisKeys.ServiceFreezeFlag, "true", time.Value);
                return StatusCode(200, new { message = $"Service freezed until {DateTime.UtcNow + time}" });
            }

            await _redisCache.DeleteCache(_redisKeys.ServiceFreezeFlag);
            return StatusCode(200, new { message = "Service unfreezed" });
        }
    }
}
