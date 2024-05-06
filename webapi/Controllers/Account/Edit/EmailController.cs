using application.Abstractions.Endpoints.Account;
using application.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/email")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class EmailController(IEmailService service, IUserInfo userInfo) : ControllerBase
    {
        [HttpPost("send/current")]
        public async Task<IActionResult> SendCurrent([FromQuery] string password)
        {
            var response = await service.StartEmailChangeProcess(password, userInfo.UserId);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("verify/current")]
        public async Task<IActionResult> VerifyCurrent([FromQuery] string email, [FromQuery] int code)
        {
            var response = await service.ConfirmOldEmail(email, code, userInfo.UserId);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("verify/new")]
        public async Task<IActionResult> Update([FromQuery] int code)
        {
            var response = await service.ConfirmNewEmailAndUpdate(code, userInfo.UserId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });

            HttpContext.Response.Cookies.Delete(ImmutableData.JWT_COOKIE_KEY);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
