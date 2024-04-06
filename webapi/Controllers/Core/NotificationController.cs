using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Core.Data_Handlers;

namespace webapi.Controllers.Core
{
    [Route("api/core/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController(
        IRepository<NotificationModel> notificationRepository,
        ICacheHandler<NotificationModel> cache,
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
                var notification = await cache.CacheAndGet(new NotificationObject(cacheKey, userInfo.UserId, notificationId));
                if (notification is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { notification });
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
                var notifications = await cache.CacheAndGetRange(new NotificationRangeObject(cacheKey, userInfo.UserId, skip, count, byDesc, priority, isChecked));

                return StatusCode(200, new { notifications });
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

        [HttpDelete("{notificationId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                var notification = await notificationRepository
                    .DeleteByFilter(query => query.Where(n => n.notification_id.Equals(notificationId) && n.user_id.Equals(userInfo.UserId)));
                
                if (notification is not null)
                    await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{userInfo.UserId}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
