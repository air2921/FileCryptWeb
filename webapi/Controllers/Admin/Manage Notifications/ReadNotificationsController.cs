using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.DB.SQL;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Notifications
{
    [Route("api/admin/notifications")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadNotificationsController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;
        private readonly IRead<NotificationModel> _read;

        public ReadNotificationsController(
            FileCryptDbContext dbContext,
            IUserInfo userInfo,
            ILogger<DeleteNotificationController> logger,
            IRead<NotificationModel> read)
        {
            _dbContext = dbContext;
            _userInfo = userInfo;
            _logger = logger;
            _read = read;
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetOneNotification([FromRoute] int notificationId)
        {
            try
            {
                var notification = await _read.ReadById(notificationId, null);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested notification information #{notificationId}");

                return StatusCode(200, new { notification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications()
        {
            try
            {
                var notification = await _read.ReadAll();
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about all notifications");

                return StatusCode(200, new { notification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/{userId}")]
        public async Task<IActionResult> ReadAllUserNotifications([FromRoute] int userId)
        {
            try
            {
                var notifications = await _dbContext.Notifications
                    .Where(n => n.sender_id == userId || n.receiver_id == userId)
                    .OrderByDescending(n => n.send_time)
                    .ToListAsync();

                if (notifications is null)
                    return StatusCode(404, new { message = ExceptionNotificationMessages.NoOneNotificationNotFound });

                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about all notifications user#{userId}");

                return StatusCode(200, new { notifications });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
