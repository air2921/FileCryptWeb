using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly IRead<NotificationModel> _readNotification;
        private readonly IDelete<NotificationModel> _deleteNotification;

        public NotificationController(FileCryptDbContext dbContext, IUserInfo userInfo, IRead<NotificationModel> readNotification, IDelete<NotificationModel> deleteNotification)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _readNotification = readNotification;
            _deleteNotification = deleteNotification;
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotification([FromQuery] int notificationId)
        {
            try
            {
                var notification = await _readNotification.ReadById(notificationId, null);

                if (notification.sender_id != _userInfo.UserId || notification.receiver_id != _userInfo.UserId)
                    return StatusCode(404);

                return StatusCode(200, new { notification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] bool? sended = null)
        {
            var query = _dbContext.Notifications.OrderByDescending(n => n.send_time).AsQueryable();
            var notifications = new List<NotificationModel>();

            switch (sended)
            {
                case true:
                    notifications = await query.Where(n => n.sender_id == _userInfo.UserId).ToListAsync();
                    break;
                case false:
                    notifications = await query.Where(n => n.receiver_id == _userInfo.UserId).ToListAsync();
                    break;
                default:
                    notifications = await query.Where(n => n.receiver_id == _userInfo.UserId || n.sender_id == _userInfo.UserId).ToListAsync();
                    break;
            }

            if (notifications is null)
                return StatusCode(404, new { message = ExceptionNotificationMessages.NoOneNotificationNotFound });

            return StatusCode(200, new { notifications });
        }

        [HttpDelete("{notificationId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                await _deleteNotification.DeleteById(notificationId, _userInfo.UserId);

                return StatusCode(200);
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
