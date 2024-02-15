using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/links")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_LinkController : ControllerBase
    {
        private readonly IRepository<LinkModel> _linkRepository;
        private readonly ISorting _sorting;
        private readonly ILogger<Admin_LinkController> _logger;

        public Admin_LinkController(
            IRepository<LinkModel> linkRepository,
            ISorting sorting,
            ILogger<Admin_LinkController> logger)
        {
            _linkRepository = linkRepository;
            _sorting = sorting;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetLink([FromQuery] int? linkId, [FromQuery] string? token)
        {
            try
            {
                LinkModel link = null;

                if (linkId.HasValue)
                {
                    link = await _linkRepository.GetById(linkId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(token))
                {
                    link = await _linkRepository.GetByFilter(query => query.Where(l => l.u_token.Equals(token)));
                }

                if (link is null)
                    return StatusCode(404);

                return StatusCode(200, new { link });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetRangeLinks([FromQuery] int? userId,
            [FromQuery] int? skip, [FromQuery] int? count,
            [FromQuery] bool byDesc, [FromQuery] bool? expired)
        {
            try
            {
                return StatusCode(200, new { links = await _linkRepository
                    .GetAll(_sorting.SortLinks(userId, skip, count, byDesc, expired)) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLink([FromQuery] int? linkId, [FromQuery] string? token)
        {
            try
            {
                if (linkId.HasValue)
                {
                    await _linkRepository.Delete(linkId.Value);
                    return StatusCode(204);
                }
                else if (!string.IsNullOrWhiteSpace(token))
                {
                    await _linkRepository.DeleteByFilter(query => query.Where(l => l.u_token.Equals(token)));
                    return StatusCode(204);
                }

                return StatusCode(404);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("many")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeLinks([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await _linkRepository.DeleteMany(identifiers);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
