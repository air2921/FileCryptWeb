using application.Abstractions.Endpoints.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/username")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class UsernameController(IUsernameService service, IUserInfo userInfo) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Update([FromQuery] string username)
        {
            var response = await service.UpdateUsername(username, userInfo.UserId);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
