using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly ISorting _sorting;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;

        public NotificationController(
            IRepository<NotificationModel> notificationRepository,
            ISorting sorting,
            IRedisCache redisCache,
            IUserInfo userInfo)
        {
            _notificationRepository = notificationRepository;
            _sorting = sorting;
            _redisCache = redisCache;
            _userInfo = userInfo;
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            try
            {
                var cacheKey = $"Notifications_{_userInfo.UserId}_{notificationId}";

                var cacheNotification = await _redisCache.GetCachedData(cacheKey);
                if (cacheNotification is not null)
                {
                    var cacheResult = JsonConvert.DeserializeObject<NotificationModel>(cacheNotification);

                    return StatusCode(200, new { notification = cacheResult });
                }

                var notification = await _notificationRepository.GetByFilter
                    (query => query.Where(n => n.notification_id.Equals(notificationId) && n.receiver_id.Equals(_userInfo.UserId)));

                if (notification is null)
                    return StatusCode(404);

                await _redisCache.CacheData(cacheKey, notification, TimeSpan.FromMinutes(10));

                return StatusCode(200, new { notification });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] string? priority,
            [FromQuery] bool? isChecked)
        {
            try
            {
                var cacheKey = $"Notifications_{_userInfo.UserId}_{skip}_{count}_{byDesc}_{priority}_{isChecked}";

                var cacheNotifications = await _redisCache.GetCachedData(cacheKey);
                if (cacheNotifications is not null)
                    return StatusCode(200, new { notifications = JsonConvert.DeserializeObject<IEnumerable<NotificationModel>>(cacheNotifications) });

                var notifications = await _notificationRepository.GetAll(_sorting.SortNotifications(_userInfo.UserId, skip, count, byDesc, priority, isChecked));

                await _redisCache.CacheData(cacheKey, notifications, TimeSpan.FromMinutes(3));

                return StatusCode(200, new { notifications });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{notificationId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                await _notificationRepository.DeleteByFilter(query => query.Where(n => n.notification_id.Equals(notificationId) && n.receiver_id.Equals(_userInfo.UserId)));
                await _redisCache.DeteteCacheByKeyPattern($"Notifications_{_userInfo.UserId}");

                return StatusCode(200);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
