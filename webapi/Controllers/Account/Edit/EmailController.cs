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
    [Route("api/account/edit/email")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        #region fields and constuctor

        private readonly string EMAIL;
        private readonly string OLD_EMAIL_CODE;
        private readonly string NEW_EMAIL_CODE;

        private readonly ITransaction<UserModel> _transaction;
        private readonly IDataManagament _dataManagament;
        private readonly IValidator _validator;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerate _generate;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;

        public EmailController(
            [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] ITransaction<UserModel> transaction,
            [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] IDataManagament dataManagament,
            [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] IValidator validator,
            IRepository<UserModel> userRepository,
            IEmailSender emailSender,
            IPasswordManager passwordManager,
            IGenerate generate,
            ITokenService tokenService,
            IUserInfo userInfo)
        {
            _transaction = transaction;
            _dataManagament = dataManagament;
            _validator = validator;
            _userRepository = userRepository;
            _emailSender = emailSender;
            _passwordManager = passwordManager;
            _generate = generate;
            _tokenService = tokenService;
            _userInfo = userInfo;
            EMAIL = $"EmailController_Email#{_userInfo.UserId}";
            OLD_EMAIL_CODE = $"EmailController_ConfirmationCode_OldEmail#{_userInfo.UserId}";
            NEW_EMAIL_CODE = $"EmailController_ConfirmationCode_NewEmail#{_userInfo.UserId}";
        }

        #endregion

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
                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!_passwordManager.CheckPassword(password, user.password))
                    return StatusCode(401, new { message = Message.INCORRECT });

                int code = _generate.GenerateSixDigitCode();
                await _emailSender.SendMessage(new EmailDto
                {
                    username = _userInfo.Username,
                    email = _userInfo.Email,
                    subject = EmailMessage.ConfirmOldEmailHeader,
                    message = EmailMessage.ConfirmOldEmailBody + code
                });

                await _dataManagament.SetData(OLD_EMAIL_CODE, code);

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

                if (!_validator.IsValid(await _dataManagament.GetData(OLD_EMAIL_CODE), code))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetByFilter(query => query.Where(u => u.email.Equals(email)));
                if (user is not null)
                    return StatusCode(409, new { message = Message.CONFLICT });

                int confirmationCode = _generate.GenerateSixDigitCode();
                await _emailSender.SendMessage(new EmailDto()
                {
                    username = _userInfo.Username,
                    email = email,
                    subject = EmailMessage.ConfirmNewEmailHeader,
                    message = EmailMessage.ConfirmNewEmailBody + confirmationCode
                });

                await _dataManagament.SetData(NEW_EMAIL_CODE, confirmationCode);
                await _dataManagament.SetData(EMAIL, email);

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
                string? email = (string)await _dataManagament.GetData(EMAIL);

                if (email is null || !_validator.IsValid(await _dataManagament.GetData(NEW_EMAIL_CODE), code))
                    return StatusCode(400, new { message = Message.INCORRECT });

                var user = await _userRepository.GetById(_userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _transaction.CreateTransaction(user, email);
                await _tokenService.UpdateJwtToken();
                await _dataManagament.DeleteData(_userInfo.UserId);

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
                _tokenService.DeleteTokens();
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}