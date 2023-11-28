using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Notifications
{
    [Route("api/admin/notifications/read")]
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

        [HttpGet("one")]
        public async Task<IActionResult> GetOneNotification([FromBody] int id)
        {
            try
            {
                var notification = await _read.ReadById(id, null);

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

        [HttpGet("all/user")]
        public async Task<IActionResult> ReadAllUserNotifications([FromBody] int userID)
        {
            try
            {
                var notifications = await _dbContext.Notifications.Where(o => o.sender_id == userID && o.receiver_id == userID).ToListAsync();
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
