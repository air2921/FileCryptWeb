﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Account;
using webapi.Third_Party_Services.Abstractions;

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    [EntityExceptionFilter]
    public class AuthSessionController(
        ISessionHelpers sessionHelper,
        [FromKeyedServices(ImplementationKey.ACCOUNT_SESSION_SERVICE)] IDataManagement dataManagament,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IPasswordManager passwordManager,
        ITokenService tokenService,
        IGenerate generate) : ControllerBase
    {
        private readonly string USER_OBJECT = "AuthSessionController_UserObject_Email:";

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Login(AuthDTO userDTO)
        {
            try
            {
                var user = await userRepository.GetByFilter(new UserByEmailSpec(userDTO.email.ToLowerInvariant()));
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (user.is_blocked)
                    return StatusCode(403, new { message = Message.BLOCKED });

                if (!passwordManager.CheckPassword(userDTO.password, user.password))
                    return StatusCode(401, new { message = Message.INCORRECT });

                if (!user.is_2fa_enabled)
                    return await sessionHelper.CreateTokens(user, HttpContext);

                int code = generate.GenerateSixDigitCode();
                await emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.Verify2FaHeader,
                    message = EmailMessage.Verify2FaBody + code
                });
                await dataManagament.SetData($"{USER_OBJECT}{user.email}", new UserContextObject 
                {
                    UserId = user.id,   
                    Code = passwordManager.HashingPassword(code.ToString())
                });

                return StatusCode(200, new { message = Message.EMAIL_SENT, confirm = true });
            }
            catch (SmtpClientException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("verify/2fa")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> VerifyTwoFA([FromQuery] int code, [FromQuery] string email)
        {
            var userContext = (UserContextObject)await dataManagament.GetData($"{USER_OBJECT}{email.ToLowerInvariant()}");
            if (userContext is null)
                return StatusCode(404, new { message = Message.TASK_TIMED_OUT });

            if (!passwordManager.CheckPassword(code.ToString(), userContext.Code))
                return StatusCode(422, new { message = Message.INCORRECT });

            var user = await userRepository.GetById(userContext.UserId);
            if (user is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return await sessionHelper.CreateTokens(user, HttpContext);
        }

        [HttpPut("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Logout()
        {
            await sessionHelper.RevokeToken(HttpContext);
            tokenService.DeleteTokens();

            return StatusCode(200);
        }

        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult AuthCheck()
        {
            return StatusCode(200);
        }
    }
}
