﻿using application.Master_Services.Account.Edit;
using application.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/username")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class UsernameController(UsernameService service, IUserInfo userInfo) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Update([FromQuery] string username)
        {
            var response = await service.UpdateUsername(username, userInfo.UserId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });

            HttpContext.Response.Cookies.Delete(ImmutableData.JWT_COOKIE_KEY);
            HttpContext.Response.Cookies.Delete(ImmutableData.USERNAME_COOKIE_KEY);
            return StatusCode(response.Status, new { message = response.Message });
        }
    }
}
