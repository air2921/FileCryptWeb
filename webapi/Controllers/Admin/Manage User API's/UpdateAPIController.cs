using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class UpdateAPIController : ControllerBase
    {
        private readonly IUpdate<ApiModel> _update;

        public UpdateAPIController(IUpdate<ApiModel> update)
        {
            _update = update;
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateAPI([FromBody] ApiModel apiModel)
        {
            try
            {
                await _update.Update(apiModel, true);

                return StatusCode(200, new { apiModel });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
