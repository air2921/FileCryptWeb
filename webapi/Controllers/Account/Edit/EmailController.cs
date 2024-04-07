using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Third_Party_Services.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/email")]
    [ApiController]
    [Authorize]
    public class EmailController(
        [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IPasswordManager passwordManager,
        IGenerate generate,
        ITokenService tokenService,
        IUserInfo userInfo) : ControllerBase
    {
        private readonly string EMAIL = $"EmailController_Email#{userInfo.UserId}";
        private readonly string OLD_EMAIL_CODE = $"EmailController_ConfirmationCode_OldEmail#{userInfo.UserId}";
        private readonly string NEW_EMAIL_CODE = $"EmailController_ConfirmationCode_NewEmail#{userInfo.UserId}";

        [HttpPost("start")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> StartEmailChangeProcess([FromQuery] string password)
        {
            try
            {
                var user = await userRepository.GetById(userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!passwordManager.CheckPassword(password, user.password))
                    return StatusCode(401, new { message = Message.INCORRECT });

                int code = generate.GenerateSixDigitCode();
                await emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.ConfirmOldEmailHeader,
                    message = EmailMessage.ConfirmOldEmailBody + code
                });

                await dataManagament.SetData(OLD_EMAIL_CODE, code);

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

        [HttpPost("confirm/old")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> ConfirmOldEmail([FromQuery] string email, [FromQuery] int code)
        {
            try
            {
                email = email.ToLowerInvariant();

                if (!validator.IsValid(await dataManagament.GetData(OLD_EMAIL_CODE), code))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await userRepository.GetByFilter(new UserByEmailSpec(email));
                if (user is not null)
                    return StatusCode(409, new { message = Message.CONFLICT });

                int confirmationCode = generate.GenerateSixDigitCode();
                await emailSender.SendMessage(new EmailDto()
                {
                    username = userInfo.Username,
                    email = email,
                    subject = EmailMessage.ConfirmNewEmailHeader,
                    message = EmailMessage.ConfirmNewEmailBody + confirmationCode
                });

                await dataManagament.SetData(NEW_EMAIL_CODE, confirmationCode);
                await dataManagament.SetData(EMAIL, email);

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

        [HttpPut("confirm/new")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        [ProducesResponseType(typeof(object), 206)]
        public async Task<IActionResult> ConfirmAndUpdateNewEmail([FromQuery] int code)
        {
            try
            {
                string? email = (string)await dataManagament.GetData(EMAIL);

                if (email is null || !validator.IsValid(await dataManagament.GetData(NEW_EMAIL_CODE), code))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await userRepository.GetById(userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await transaction.CreateTransaction(user, email);
                await dataManagament.DeleteData(userInfo.UserId);
                await tokenService.UpdateJwtToken();

                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                tokenService.DeleteTokens();
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}