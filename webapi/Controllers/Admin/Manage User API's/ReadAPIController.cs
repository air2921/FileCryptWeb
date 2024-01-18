using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using webapi.Controllers.Admin.Manage_Notifications;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_User_s_API
{
    [Route("api/admin/api")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class ReadAPIController : ControllerBase
    {
        private readonly IRead<ApiModel> _read;
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteNotificationController> _logger;
        private readonly FileCryptDbContext _dbContext;

        public ReadAPIController(
            IRead<ApiModel> read,
            IUserInfo userInfo,
            ILogger<DeleteNotificationController> logger,
            FileCryptDbContext dbContext)
        {
            _read = read;
            _userInfo = userInfo;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApiSettings([FromRoute] int id, [FromQuery] bool byRelation)
        {
            try
            {
                var api = await _read.ReadById(id, byRelation);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} requested API settings from user#{id}");

                return StatusCode(200, new { api });
            }
            catch (ApiException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("key")]
        public async Task<IActionResult> GetApiByApiKey([FromQuery] string apiKey)
        {
            var api = await _dbContext.API.FirstOrDefaultAsync(a => a.api_key == apiKey);

            if (api is null)
                return StatusCode(404, new { message = "API key doesn't exists" });

            _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} requested API settings by key value: '{apiKey}'");

            return StatusCode(200, new { api });
        }
    }
}
