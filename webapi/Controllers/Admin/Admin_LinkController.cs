using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Exceptions;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/links")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_LinkController(
        IRepository<LinkModel> linkRepository,
        ILogger<Admin_LinkController> logger) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(LinkModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetLink([FromQuery] int? linkId, [FromQuery] string? token)
        {
            try
            {
                LinkModel link = null;

                if (linkId.HasValue)
                    link = await linkRepository.GetById(linkId.Value);
                else if (!string.IsNullOrWhiteSpace(token))
                    link = await linkRepository.GetByFilter(new RecoveryTokenByTokenSpec(token));

                if (link is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { link });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<LinkModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetRangeLinks([FromQuery] int? userId,
            [FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] bool? expired)
        {
            try
            {
                return StatusCode(200, new { links = await linkRepository
                    .GetAll(new LinksSortSpec(userId, skip, count, byDesc, expired)) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteLink([FromQuery] int linkId)
        {
            try
            {
                await linkRepository.Delete(linkId);
                return StatusCode(204, new { message = Message.NOT_FOUND });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteRangeLinks([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await linkRepository.DeleteMany(identifiers);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
