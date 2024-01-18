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
        public async Task<IActionResult> GetAllNotifications([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count)
        {
            try
            {
                var notification = await _read.ReadAll(userId, skip, count);
                _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} requested information about notifications, skipped {skip} and quntity requested {count}");

                return StatusCode(200, new { notification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
