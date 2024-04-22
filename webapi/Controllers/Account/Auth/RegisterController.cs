﻿using application.Abstractions.Endpoints.Account;
using application.DTO.Outer;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Account.Auth
{
    [Route("api/auth")]
    [ApiController]
    [ValidateAntiForgeryToken]
    public class RegisterController(IRegistrationService service) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> StartRegistration([FromBody] RegisterDTO registerDTO)
        {
            var response = await service.Registration(registerDTO);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromQuery] string email, [FromQuery] int code)
        {
            var response = await service.VerifyAccount(code, email);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
