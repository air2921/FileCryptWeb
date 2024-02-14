using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/api")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_ApiController : ControllerBase
    {
        private readonly IRepository<ApiModel> _apiRepository;

        public Admin_ApiController(IRepository<ApiModel> apiRepository)
        {
            _apiRepository = apiRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetApi([FromQuery] int? apiId, [FromQuery] string? key)
        {
            try
            {
                ApiModel api = null;

                if (apiId.HasValue)
                {
                    api = await _apiRepository.GetById(apiId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(key))
                {
                    api = await _apiRepository.GetByFilter(query => query.Where(a => a.api_key.Equals(key)));
                }

                if (api is null)
                    return StatusCode(404);

                return StatusCode(200, new { api });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetRangeApi([FromQuery] int userId)
        {
            try
            {
                return StatusCode(200, new { api = await _apiRepository.GetAll(query => query.Where(a => a.user_id.Equals(userId))) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{apiId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApi([FromRoute] int apiId)
        {
            try
            {
                await _apiRepository.Delete(apiId);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("many")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRangeApi([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await _apiRepository.DeleteMany(identifiers);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
