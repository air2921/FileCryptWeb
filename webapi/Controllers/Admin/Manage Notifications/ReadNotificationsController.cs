using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
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
        private readonly IRead<NotificationModel> _read;

        public ReadNotificationsController(FileCryptDbContext dbContext, IRead<NotificationModel> read)
        {
            _dbContext = dbContext;
            _read = read;
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetOneNotification([FromRoute] int notificationId)
        {
            try
            {
                var notification = await _read.ReadById(notificationId, null);

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
                    .Where(n => n.sender_id == userId && n.receiver_id == userId)
                    .OrderByDescending(n => n.send_time)
                    .ToListAsync();

                if (notifications is null)
                    return StatusCode(404, new { message = ExceptionNotificationMessages.NoOneNotificationNotFound });

                return StatusCode(200, new { notifications });
            }
            catch (OfferException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
