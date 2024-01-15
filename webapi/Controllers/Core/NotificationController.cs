using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly IRead<NotificationModel> _readNotification;
        private readonly IDelete<NotificationModel> _deleteNotification;

        private const string NOTIFICATIONS = "Cache_Notification_List";

        public NotificationController(
            FileCryptDbContext dbContext,
            IRedisCache redisCache,
            IUserInfo userInfo,
            IRead<NotificationModel> readNotification,
            IDelete<NotificationModel> deleteNotification)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
            _userInfo = userInfo;
            _readNotification = readNotification;
            _deleteNotification = deleteNotification;
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            var cacheKey = $"Notification_{notificationId}";

            try
            {
                var cacheNotification = await _redisCache.GetCachedData(cacheKey);
                if (cacheNotification is not null)
                {
                    var cacheResult = JsonConvert.DeserializeObject<NotificationModel>(cacheNotification);

                    if (cacheResult!.receiver_id != _userInfo.UserId)
                        return StatusCode(404);

                    return StatusCode(200, new { notification = cacheResult });
                }    

                var notification = await _readNotification.ReadById(notificationId, null);

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                await _redisCache.CacheData(cacheKey, JsonConvert.SerializeObject(notification, settings), TimeSpan.FromMinutes(10));

                if (notification.receiver_id != _userInfo.UserId)
                    return StatusCode(404);

                return StatusCode(200, new { notification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int skip, [FromQuery] int count)
        {
            var cacheKey = $"Notifications_{_userInfo.UserId}_{skip}_{count}";
            bool clearCache = HttpContext.Session.GetString(NOTIFICATIONS) is not null ? bool.Parse(HttpContext.Session.GetString(NOTIFICATIONS)) : true;

            if (clearCache)
            {
                await _redisCache.DeleteCache(cacheKey);
                HttpContext.Session.SetString(NOTIFICATIONS, false.ToString());
            }

            var cacheNotifications = await _redisCache.GetCachedData(cacheKey);
            if (cacheNotifications is not null)
                return StatusCode(200, new { notifications = JsonConvert.DeserializeObject<IEnumerable<NotificationModel>>(cacheNotifications) });

            var notifications = await _dbContext.Notifications.OrderByDescending(n => n.send_time)
                .Where(n => n.receiver_id == _userInfo.UserId)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            await _redisCache.CacheData(cacheKey, JsonConvert.SerializeObject(notifications, settings), TimeSpan.FromMinutes(5));

            return StatusCode(200, new { notifications });
        }

        [HttpDelete("{notificationId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                await _deleteNotification.DeleteById(notificationId, _userInfo.UserId);
                HttpContext.Session.SetString(NOTIFICATIONS, true.ToString());

                return StatusCode(200);
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
