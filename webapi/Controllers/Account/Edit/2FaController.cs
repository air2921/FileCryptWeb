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
    public class _2FaController : ControllerBase
    {
        #region fields and constructor

        private readonly string CODE;
        private readonly ITransaction<UserModel> _transaction;
        private readonly IDataManagament _dataManagament;
        private readonly IValidator _validator;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IPasswordManager _passwordManager;
        private readonly IUserInfo _userInfo;
        private readonly IGenerate _generate;

        public _2FaController(
            [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] ITransaction<UserModel> transaction,
            [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IDataManagament dataManagament,
            [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IValidator validator,
            IEmailSender emailSender,
            IRepository<UserModel> userRepository,
            IPasswordManager passwordManager,
            IUserInfo userInfo,
            IGenerate generate)
        {
            _transaction = transaction;
            _dataManagament = dataManagament;
            _emailSender = emailSender;
            _validator = validator;
            _userRepository = userRepository;
            _passwordManager = passwordManager;
            _userInfo = userInfo;
            _generate = generate;
            CODE = $"_2FaController_VerificationCode#{_userInfo.UserId}";
        }

        #endregion

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
                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!_passwordManager.CheckPassword(password, user.password))
                    return StatusCode(401, new { message = Message.INCORRECT });

                int code = _generate.GenerateSixDigitCode();
                await _emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.Change2FaHeader,
                    message = EmailMessage.Change2FaBody + code
                });

                await _dataManagament.SetData(CODE, code);

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
                if (_validator.IsValid(await _dataManagament.GetData(CODE), code))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _transaction.CreateTransaction(user, enable);
                await _dataManagament.DeleteData(_userInfo.UserId);

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
