using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Notifications
{
    [Route("api/admin/notifications")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class DeleteNotificationController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;
        private readonly IDelete<NotificationModel> _deleteNotification;

        public DeleteNotificationController(IUserInfo userInfo, ILogger<DeleteNotificationController> logger, IDelete<NotificationModel> deleteNotification)
        {
            _userInfo = userInfo;
            _logger = logger;
            _deleteNotification = deleteNotification;
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                await _deleteNotification.DeleteById(notificationId);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} deleted notification #{notificationId} from db");

                return StatusCode(200, new { message = SuccessMessage.SuccessDeletedNotification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
