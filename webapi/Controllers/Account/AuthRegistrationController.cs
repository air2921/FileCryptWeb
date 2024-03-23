using Microsoft.AspNetCore.Mvc;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
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
    public class AuthRegistrationController(
        [FromKeyedServices(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE)] ITransaction<UserObject> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IPasswordManager passwordManager,
        IGenerate generate) : ControllerBase
    {
        private readonly string USER_OBJECT = "AuthRegistrationController_UserObject_Email:";

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Registration([FromBody] RegisterDTO userDTO)
        {
            try
            {
                userDTO.email = userDTO.email.ToLowerInvariant();
                int code = generate.GenerateSixDigitCode();

                if (!validator.IsValid(userDTO))
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                var user = await userRepository.GetByFilter(query => query.Where(u => u.email.Equals(userDTO.email)));
                if (user is not null)
                    return StatusCode(409, new { message = Message.USER_EXISTS });

                await emailSender.SendMessage(new EmailDto
                {
                    username = userDTO.username,
                    email = userDTO.email,
                    subject = EmailMessage.VerifyEmailHeader,
                    message = EmailMessage.VerifyEmailBody + code
                });
                await dataManagament.SetData($"{USER_OBJECT}{userDTO.email}", new UserObject
                {
                    Email = userDTO.email,
                    Password = userDTO.password,
                    Username = userDTO.username,
                    Role = Role.User.ToString(),
                    Flag2Fa = userDTO.is_2fa_enabled,
                    Code = code.ToString()
                });

                return StatusCode(200, new { message = Message.EMAIL_SENT });
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

        [HttpPost("verify")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> VerifyAccount([FromQuery] int code, [FromQuery] string email)
        {
            try
            {
                var user = (UserObject)await dataManagament.GetData($"{USER_OBJECT}{email.ToLowerInvariant()}");
                if (user is null)
                    return StatusCode(404, new { message = Message.TASK_TIMED_OUT });

                if (!passwordManager.CheckPassword(code.ToString(), user.Code))
                    return StatusCode(422, new { message = Message.INCORRECT });

                await transaction.CreateTransaction(user);

                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
