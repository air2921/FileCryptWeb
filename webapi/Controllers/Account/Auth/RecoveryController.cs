using application.Abstractions.Endpoints.Account;
using application.DTO.Outer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Account.Auth
{
    [Route("api/auth")]
    [ApiController]
    [ValidateAntiForgeryToken]
    public class RecoveryController(IRecoveryService service) : ControllerBase
    {
        [HttpPost("send/ticket")]
        public async Task<IActionResult> SendTicket([FromQuery] string email)
        {
            var response = await service.SendTicket(email);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset(RecoveryDTO dto)
        {
            var response = await service.ChangePassword(dto);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
