using application.Master_Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/mime")]
    [ApiController]
    [Authorize(Policy = "RequireAdminPolicy")]
    public class _MimeController(Admin_MimeService service) : ControllerBase
    {
        [HttpPost("{mime}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMime([FromRoute] string mime)
        {
            var response = await service.CreateOne(mime);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("range")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRangeMimes([FromBody] IEnumerable<string> mimes)
        {
            var response = await service.CreateRange(mimes);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("template")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTemplate()
        {
            var response = await service.CreateRangeTemplate();
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpGet("{mimeId}")]
        public async Task<IActionResult> GetMime([FromRoute] int mimeId)
        {
            var response = await service.GetOne(mimeId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { mime = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeMimes([FromQuery] int skip, [FromQuery] int count)
        {
            var response = await service.GetRange(skip, count);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { mimes = response.ObjectData });
        }

        [HttpDelete("{mimeId}")]
        [Authorize(Roles = "HighestAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMime([FromRoute] int mimeId)
        {
            var response = await service.DeleteOne(mimeId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        [HttpDelete("range")]
        [Authorize(Roles = "HighestAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeMimes([FromBody] IEnumerable<int> identifiers)
        {
            var response = await service.DeleteRange(identifiers);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
