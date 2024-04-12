using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Helpers;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/notifications")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [EntityExceptionFilter]
    public class Admin_NotificationController(IRepository<NotificationModel> notificationRepository, IRedisCache redisCache) : ControllerBase
    {
        [HttpGet("{notificationId}")]
        [ProducesResponseType(typeof(NotificationModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            var notification = await notificationRepository.GetById(notificationId);
            if (notification is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return StatusCode(200, new { notification });
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<NotificationModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRangeNotification([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count, bool byDesc)
        {
            return StatusCode(200, new { notification = await notificationRepository
                .GetAll(new NotificationsSortSpec(userId, skip, count, byDesc, null, null))});
        }

        [HttpDelete("{notificationId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            var notification = await notificationRepository.Delete(notificationId);
            if (notification is not null)
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{notification.user_id}");

            return StatusCode(204);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteRangeNotifications([FromBody] IEnumerable<int> identifiers)
        {
            var notificationList = await notificationRepository.DeleteMany(identifiers);
            await redisCache.DeleteRedisCache(notificationList, ImmutableData.NOTIFICATIONS_PREFIX, item => item.user_id);
            return StatusCode(204);
        }
    }
}
