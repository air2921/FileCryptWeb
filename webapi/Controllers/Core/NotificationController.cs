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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification([FromRoute] int id)
        {
            try
            {
                var notification = await _readNotification.ReadById(id, null);

                return StatusCode(200, new { notification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all/{received}")]
        public async Task<IActionResult> GetAll([FromRoute] bool received)
        {
            var query = _dbContext.Notifications.OrderByDescending(n => n.send_time).AsQueryable();
            var notifications = new List<NotificationModel>();

            switch (received)
            {
                case true:
                    notifications = await query.Where(n => n.receiver_id == _userInfo.UserId).ToListAsync();
                    break;
                case false:
                    notifications = await query.Where(n => n.receiver_id == _userInfo.UserId && n.sender_id == _userInfo.UserId).ToListAsync();
                    break;
            }

            if (notifications is null || notifications.Count == 0)
                return StatusCode(404, new { message = ExceptionNotificationMessages.NoOneNotificationNotFound });

            return StatusCode(200, new { notifications });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification([FromRoute] int id)
        {
            try
            {
                await _deleteNotification.DeleteById(id);

                return StatusCode(200);
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
