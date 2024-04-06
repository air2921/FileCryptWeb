using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/2fa")]
    [ApiController]
    [Authorize]
    public class _2FaController(
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IValidator validator,
        IEmailSender emailSender,
        IRepository<UserModel> userRepository,
        IPasswordManager passwordManager,
        IUserInfo userInfo,
        IGenerate generate) : ControllerBase
    {
        private readonly string CODE = $"_2FaController_VerificationCode#{userInfo.UserId}";

        [HttpPost("start")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SendVerificationCode([FromQuery] string password)
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
                    subject = EmailMessage.Change2FaHeader,
                    message = EmailMessage.Change2FaBody + code
                });

                await dataManagament.SetData(CODE, code);

                return StatusCode(200);
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

        [HttpPut("confirm/{enable}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> Update2FaState([FromQuery] int code, [FromRoute] bool enable)
        {
            try
            {
                if (!validator.IsValid(await dataManagament.GetData(CODE), code))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await userRepository.GetById(userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await transaction.CreateTransaction(user, enable);
                await dataManagament.DeleteData(userInfo.UserId);

                return StatusCode(200);
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
        }
    }
}
