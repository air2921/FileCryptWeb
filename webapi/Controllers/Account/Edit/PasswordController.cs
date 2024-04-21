using application.Abstractions.Endpoints.Account;
using application.DTO.Outer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/password")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class PasswordController(IPasswordService service, IUserInfo userInfo) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Change([FromBody] PasswordDTO dto)
        {
            var response = await service.UpdatePassword(dto, userInfo.UserId);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
