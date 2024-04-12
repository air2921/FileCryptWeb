using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/links")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [EntityExceptionFilter]
    public class Admin_LinkController(
        IRepository<LinkModel> linkRepository,
        ILogger<Admin_LinkController> logger) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(LinkModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetLink([FromQuery] int linkId)
        {
            var link = await linkRepository.GetById(linkId);
            if (link is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return StatusCode(200, new { link });
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<LinkModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRangeLinks([FromQuery] int? userId,
            [FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? expired)
        {
            return StatusCode(200, new { links = await linkRepository
                .GetAll(new LinksSortSpec(userId, skip, count, byDesc, expired))});
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteLink([FromQuery] int linkId)
        {
            await linkRepository.Delete(linkId);
            return StatusCode(204);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteRangeLinks([FromBody] IEnumerable<int> identifiers)
        {
            await linkRepository.DeleteMany(identifiers);
            return StatusCode(204);
        }
    }
}
