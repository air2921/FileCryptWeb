using application.Master_Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/core/notification")]
    [ApiController]
    [Authorize]
    public class NotificationController(
        NotificationsService service,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            var response = await service.GetOne(userInfo.UserId, notificationId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { notification = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeNotifications([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] int? priority,
            [FromQuery] bool? isChecked)
        {
            var response = await service.GetRange(userInfo.UserId, skip, count, byDesc, priority, isChecked);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { notifications = response.ObjectData });
        }

        [HttpDelete("{notificationId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            var response = await service.DeleteOne(userInfo.UserId, notificationId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
