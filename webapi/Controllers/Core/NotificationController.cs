using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController(
        IRepository<NotificationModel> notificationRepository,
        ISorting sorting,
        IRedisCache redisCache,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpGet("{notificationId}")]
        [ProducesResponseType(typeof(NotificationModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.NOTIFICATIONS_PREFIX}{userInfo.UserId}_{notificationId}";

                var cacheNotification = await redisCache.GetCachedData(cacheKey);
                if (cacheNotification is not null)
                    return StatusCode(200, new { notification = JsonConvert.DeserializeObject<NotificationModel>(cacheNotification) });

                var notification = await notificationRepository.GetByFilter
                    (query => query.Where(n => n.notification_id.Equals(notificationId) && n.user_id.Equals(userInfo.UserId)));

                if (notification is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await redisCache.CacheData(cacheKey, notification, TimeSpan.FromMinutes(10));

                return StatusCode(200, new { notification });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<NotificationModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAll([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] string? priority,
            [FromQuery] bool? isChecked)
        {
            try
            {
                var cacheKey = $"{ImmutableData.NOTIFICATIONS_PREFIX}{userInfo.UserId}_{skip}_{count}_{byDesc}_{priority}_{isChecked}";

                var cacheNotifications = await redisCache.GetCachedData(cacheKey);
                if (cacheNotifications is not null)
                    return StatusCode(200, new { notifications = JsonConvert.DeserializeObject<IEnumerable<NotificationModel>>(cacheNotifications) });

                var notifications = await notificationRepository.GetAll(sorting.SortNotifications(userInfo.UserId, skip, count, byDesc, priority, isChecked));

                await redisCache.CacheData(cacheKey, notifications, TimeSpan.FromMinutes(3));

                return StatusCode(200, new { notifications });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{notificationId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                await notificationRepository.DeleteByFilter(query => query.Where(n => n.notification_id.Equals(notificationId) && n.user_id.Equals(userInfo.UserId)));
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{userInfo.UserId}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
