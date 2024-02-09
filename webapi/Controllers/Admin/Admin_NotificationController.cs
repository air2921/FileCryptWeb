using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/notifications")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_NotificationController : ControllerBase
    {
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly ISorting _sorting;

        public Admin_NotificationController(IRepository<NotificationModel> notificationRepository, ISorting sorting)
        {
            _notificationRepository = notificationRepository;
            _sorting = sorting;
        }

        [HttpGet("notificationId")]
        public async Task<IActionResult> GetNotification([FromRoute] int notificationId)
        {
            var notification = await _notificationRepository.GetById(notificationId);
            if (notification is null)
                return StatusCode(404);

            return StatusCode(200, new { notification });
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetRangeNotification([FromQuery] int? userId, [FromQuery] int? skip, [FromQuery] int? count, bool byDesc)
        {
            return StatusCode(200, new { notification = await _notificationRepository.GetAll(_sorting.SortNotifications(userId, skip, count, byDesc, null, null)) });
        }

        [HttpDelete("notificationId")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification([FromRoute] int notificationId)
        {
            try
            {
                var notification = await _notificationRepository.GetById(notificationId);
                if (notification is null)
                    return StatusCode(404);

                return StatusCode(204, new { notification });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("many")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeNotifications([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await _notificationRepository.DeleteMany(identifiers);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
