using application.Master_Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/file")]
    [ApiController]
    [Authorize(Policy = "RequireAdminPolicy")]
    public class _FileController(Admin_FileService service) : ControllerBase
    {
        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile([FromRoute] int fileId)
        {
            var response = await service.GetOne(fileId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { file = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeFiles([FromQuery] int? userId, [FromQuery] int skip,
            [FromQuery] int count, [FromQuery] bool byDesc, [FromQuery] string? category)
        {
            var response = await service.GetRange(userId, skip, count, byDesc, category);
            return StatusCode(response.Status, new { files = response.ObjectData });
        }

        [HttpDelete("{fileId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile([FromRoute] int fileId)
        {
            var response = await service.DeleteOne(fileId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeFiles([FromBody] IEnumerable<int> identifiers)
        {
            var response = await service.DeleteRange(identifiers);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
