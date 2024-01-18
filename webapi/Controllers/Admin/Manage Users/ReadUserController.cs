using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Users
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class ReadUserController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<ReadUserController> _logger;
        private readonly IRead<UserModel> _read;

        public ReadUserController(IUserInfo userInfo, ILogger<ReadUserController> logger, IRead<UserModel> read)
        {
            _userInfo = userInfo;
            _logger = logger;
            _read = read;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> ReadUser([FromRoute] int userId)
        {
            try
            {
                var user = await _read.ReadById(userId, null);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} requested information user#{userId}");

                return StatusCode(200, new { user });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ReadAllUsers([FromQuery] int skip, [FromQuery] int count)
        {
            try
            {
                var users = await _read.ReadAll(null, skip, count);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} requested information about users, skipped {skip} and quantity requested {count}");

                return StatusCode(200, new { users });
            }
            catch (UserException ex)
            {
                return StatusCode(200, new { message = ex.Message });
            }
        }
    }
}
