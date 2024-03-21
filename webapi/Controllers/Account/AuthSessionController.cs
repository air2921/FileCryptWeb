using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Account;

namespace webapi.Controllers.Account
{
    [Route("api/auth")]
    [ApiController]
    public class AuthSessionController(
        ISessionHelpers sessionHelper,
        IDataManagament dataManagament,
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
                var user = await userRepository.GetByFilter(query => query.Where(u => u.email.Equals(userDTO.email.ToLowerInvariant())));
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
            catch (OperationCanceledException ex)
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
            try
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
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await sessionHelper.RevokeToken(HttpContext);
                tokenService.DeleteTokens();

                return StatusCode(200);
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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
