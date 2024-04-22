﻿using application.Abstractions.Endpoints.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/2fa")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class TwoFaController(I2FaService service, IUserInfo userInfo) : ControllerBase
    {
        [HttpPost("send/mail")]
        public async Task<IActionResult> SendMail([FromQuery] string password)
        {
            var response = await service.SendVerificationCode(password, userInfo.UserId);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("verify/{enable}")]
        public async Task<IActionResult> Verify([FromQuery] int code, [FromRoute] bool enable)
        {
            var response = await service.UpdateState(code, enable, userInfo.UserId);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
