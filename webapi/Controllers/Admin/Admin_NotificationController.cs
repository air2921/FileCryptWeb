using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/notifications")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_NotificationController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRedisCache _redisCache;
        private readonly ISorting _sorting;

        public Admin_NotificationController(IRepository<NotificationModel> notificationRepository, IRedisCache redisCache, ISorting sorting)
        {
            _notificationRepository = notificationRepository;
            _redisCache = redisCache;
            _sorting = sorting;
        }

        #endregion

        [HttpGet("{notificationId}")]
        [ProducesResponseType(typeof(NotificationModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            try
            {
                var notification = await _notificationRepository.GetById(notificationId);
                if (notification is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { notification });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<NotificationModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRangeNotification([FromQuery] int? userId, [FromQuery] int? skip, [FromQuery] int? count, bool byDesc)
        {
            try
            {
                return StatusCode(200, new { notification = await _notificationRepository
                    .GetAll(_sorting.SortNotifications(userId, skip, count, byDesc, null, null)) });
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
                var notification = await _notificationRepository.Delete(notificationId);
                if (notification is not null)
                    await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{notification.user_id}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteRangeNotifications([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                var notificationList = await _notificationRepository.DeleteMany(identifiers);
                await _redisCache.DeleteRedisCache(notificationList, ImmutableData.NOTIFICATIONS_PREFIX, item => item.user_id);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
