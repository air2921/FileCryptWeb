using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api/get")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class ReadAPIController : ControllerBase
    {
        private readonly IRead<ApiModel> _read;
        private readonly FileCryptDbContext _dbContext;

        public ReadAPIController(IRead<ApiModel> read, FileCryptDbContext dbContext)
        {
            _read = read;
            _dbContext = dbContext;
        }

        [HttpGet("settings/{byRelation}")]
        public async Task<IActionResult> GetApiSettings(int id, [FromRoute] bool ByRelation)
        {
            try
            {
                var api = new ApiModel();

                if(ByRelation)
                {
                    api = await _read.ReadById(id, true);
                }
                else
                {
                    api = await _read.ReadById(id, false);
                }

                return StatusCode(200, new { api });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetApiByApiKey(string apiKey)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey);

            if (api is null)
                return StatusCode(404, new { message = "API key doesn't exists" });

            return StatusCode(200, new { api });
        }
    }
}
