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
    public class NotificationController : ControllerBase
    {
        #region fields and constructor

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

        #endregion

        [HttpGet("{notificationId}")]
        [ProducesResponseType(typeof(NotificationModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.NOTIFICATIONS_PREFIX}{_userInfo.UserId}_{notificationId}";

                var cacheNotification = await _redisCache.GetCachedData(cacheKey);
                if (cacheNotification is not null)
                    return StatusCode(200, new { notification = JsonConvert.DeserializeObject<NotificationModel>(cacheNotification) });

                var notification = await _notificationRepository.GetByFilter
                    (query => query.Where(n => n.notification_id.Equals(notificationId) && n.user_id.Equals(_userInfo.UserId)));

                if (notification is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _redisCache.CacheData(cacheKey, notification, TimeSpan.FromMinutes(10));

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
                var cacheKey = $"{ImmutableData.NOTIFICATIONS_PREFIX}{_userInfo.UserId}_{skip}_{count}_{byDesc}_{priority}_{isChecked}";

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
        [XSRFProtection]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                await _notificationRepository.DeleteByFilter(query => query.Where(n => n.notification_id.Equals(notificationId) && n.user_id.Equals(_userInfo.UserId)));
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{_userInfo.UserId}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
