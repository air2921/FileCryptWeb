using application.Master_Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/notification")]
    [ApiController]
    [Authorize(Policy = "RequireAdminPolicy")]
    public class _NotificationController(Admin_NotificationService service) : ControllerBase
    {
        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            var response = await service.GetOne(notificationId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { notification = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeNotifications([FromQuery] int? userId, [FromQuery] int skip,
            [FromQuery] int count, [FromQuery] bool byDesc)
        {
            var response = await service.GetRange(userId, skip, count, byDesc);
            return StatusCode(response.Status, new { notifications = response.ObjectData });
        }

        [HttpDelete("{notificationId}")]
        [Authorize(Roles = "HighestAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            var response = await service.DeleteOne(notificationId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        [HttpDelete("range")]
        [Authorize(Roles = "HighestAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeNotifications([FromBody] IEnumerable<int> identifiers)
        {
            var response = await service.DeleteRange(identifiers);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
