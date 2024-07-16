using application.Master_Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/token/recovery")]
    [ApiController]
    [Authorize(Policy = "RequireAdminPolicy")]
    public class _LinkController(Admin_LinkService service) : ControllerBase
    {
        [HttpGet("{linkId}")]
        public async Task<IActionResult> GetLink([FromRoute] int linkId)
        {
            var response = await service.GetOne(linkId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { link = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeLinks([FromQuery] int? userId, [FromQuery] int skip,
            [FromQuery] int count, [FromQuery] bool byDesc, [FromQuery] bool? expired)
        {
            var response = await service.GetRange(userId, skip, count, byDesc, expired);
            return StatusCode(response.Status, new { links = response.ObjectData });
        }

        [HttpDelete("{linkId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLink([FromRoute] int linkId)
        {
            var response = await service.DeleteOne(linkId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeLinks([FromBody] IEnumerable<int> identifiers)
        {
            var response = await service.DeleteRange(identifiers);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
