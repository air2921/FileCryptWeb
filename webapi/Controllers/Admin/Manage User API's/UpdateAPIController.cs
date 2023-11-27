using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces.SQL.API;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api/update")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class UpdateAPIController : ControllerBase
    {
        private readonly IUpdateAPI _updateAPI;

        public UpdateAPIController(IUpdateAPI updateAPI)
        {
            _updateAPI = updateAPI;
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateAPI(ApiModel apiModel)
        {
            await _updateAPI.UpdateApiSetting(apiModel);

            return StatusCode(200, new { apiModel });
        }
    }
}
