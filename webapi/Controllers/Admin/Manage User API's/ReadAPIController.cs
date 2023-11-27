using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL.API;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api/get")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class ReadAPIController : ControllerBase
    {
        private readonly IReadAPI _readAPI;

        public ReadAPIController(IReadAPI readAPI)
        {
            _readAPI = readAPI;
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetApiSettings(int id)
        {
            try
            {
                var api = await _readAPI.ReadUserApiSettings(id);

                return StatusCode(200, new { api });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserByApiKey(string apiKey)
        {
            try
            {
                var user = await _readAPI.ReadUserByApiKey(apiKey);

                return StatusCode(200, new { user });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
