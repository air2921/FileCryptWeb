using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
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
        private readonly IDelete<NotificationModel> _deleteNotification;

        public DeleteNotificationController(IDelete<NotificationModel> deleteNotification)
        {
            _deleteNotification = deleteNotification;
        }

        [HttpDelete("one")]
        public async Task<IActionResult> DeleteNotification([FromBody] int id)
        {
            try
            {
                await _deleteNotification.DeleteById(id);

                return StatusCode(200, new { message = SuccessMessage.SuccessDeletedNotification });
            }
            catch (NotificationException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
