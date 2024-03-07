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
    public class AppController : ControllerBase
    {
        #region fields and constructor

        private readonly IUserInfo _userInfo;
        private readonly ILogger<AppController> _logger;
        private readonly IRedisCache _redisCache;

        public AppController(IUserInfo userInfo, ILogger<AppController> logger, IRedisCache redisCache)
        {
            _userInfo = userInfo;
            _logger = logger;
            _redisCache = redisCache;
        }

        #endregion

        [HttpPut("freeze")]
        [XSRFProtection]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> FreezeService([FromQuery] bool flag, [FromBody] TimeSpan? time)
        {
            if (flag && time.HasValue)
            {
                await _redisCache.CacheData(ImmutableData.SERVICE_FREEZE_FLAG, true, time.Value);
                _logger.LogCritical($"{_userInfo.Username}#{_userInfo.UserId} freezed service until {DateTime.UtcNow + time}");

                return StatusCode(200, new { message = $"Service freezed until {DateTime.UtcNow + time}" });
            }

            await _redisCache.DeleteCache(ImmutableData.SERVICE_FREEZE_FLAG);
            _logger.LogCritical($"{_userInfo.Username}#{_userInfo.UserId} unfreezed service at {DateTime.UtcNow}");

            return StatusCode(200, new { message = "Service unfreezed" });
        }
    }
}
