using application.Abstractions.Endpoints.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/core/user")]
    [ApiController]
    [Authorize]
    public class UserController(
        IUserService service,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId, [FromQuery] bool own)
        {
            var response = await service.GetOne(userInfo.UserId, own ? userInfo.UserId : userId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { user = response.ObjectData });
        }

        [HttpGet("range/{username}")]
        public async Task<IActionResult> GetRangeUsers([FromRoute] string username)
        {
            var response = await service.GetRange(username);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { users = response.ObjectData });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser()
        {
            var response = await service.DeleteOne(userInfo.UserId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
