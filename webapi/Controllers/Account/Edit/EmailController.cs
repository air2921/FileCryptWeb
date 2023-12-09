using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/email")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private const string EMAIL = "Email";

        private readonly FileCryptDbContext _dbContext;
        private readonly IUpdate<UserModel> _update;
        private readonly IEmailSender<UserModel> _email;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;

        public EmailController(
            FileCryptDbContext dbContext,
            IUpdate<UserModel> update,
            IEmailSender<UserModel> email,
            IPasswordManager passwordManager,
            IGenerateSixDigitCode generateCode,
            ITokenService tokenService,
            IUserInfo userInfo,
            IValidation validation)
        {
            _dbContext = dbContext;
            _update = update;
            _email = email;
            _passwordManager = passwordManager;
            _generateCode = generateCode;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _validation = validation;
        }

        [HttpPost("old")]
        public async Task<IActionResult> StartEmailChangeProcess([FromBody] UserModel userModel)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == _userInfo.Email);
            if (user is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404);
            }

            bool IsCorrect = _passwordManager.CheckPassword(userModel.password_hash, user.password_hash);
            if (!IsCorrect)
                return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

            int code = _generateCode.GenerateSixDigitCode();
            string messageHeader = EmailMessage.ConfirmOldEmailHeader;
            string message = EmailMessage.ConfirmOldEmailBody + code;

            var newUserModel = new UserModel { username = _userInfo.Username, email = _userInfo.Email };
            await _email.SendMessage(newUserModel, messageHeader, message);

            HttpContext.Session.SetInt32(_userInfo.Email, code);

            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
        }

        [HttpPost("confirm/old")]
        public IActionResult ConfirmOldEmail([FromQuery] int code)
        {
            int correctCode = (int)HttpContext.Session.GetInt32(_userInfo.Email);

            if (!_validation.IsSixDigit(correctCode))
                return StatusCode(500, new { message = AccountErrorMessage.Error });

            if (!code.Equals(correctCode))
                return StatusCode(401, new { message = AccountErrorMessage.CodeIncorrect });

            return StatusCode(201, new { message = AccountSuccessMessage.OldEmailConfirmed });
        }

        [HttpPost("new")]
        public async Task<IActionResult> SendEmailVerificationCode([FromBody] UserModel userModel)
        {
            string email = userModel.email.ToLowerInvariant();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
            if (user is not null)
                return StatusCode(409, new { message = AccountErrorMessage.UserExists });

            int code = _generateCode.GenerateSixDigitCode();
            string messageHeader = EmailMessage.ConfirmNewEmailHeader;
            string message = EmailMessage.ConfirmNewEmailBody + code;

            var newUserModel = new UserModel { username = _userInfo.Username, email = email };
            await _email.SendMessage(newUserModel, messageHeader, message);

            HttpContext.Session.SetInt32(_userInfo.UserId.ToString(), code);
            HttpContext.Session.SetString(EMAIL, email);

            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
        }

        [HttpPut("confirm/new")]
        public async Task<IActionResult> ConfirmAndUpdateNewEmail([FromQuery] int code)
        {
            try
            {
                int correctCode = (int)HttpContext.Session.GetInt32(_userInfo.UserId.ToString());
                string? email = HttpContext.Session.GetString(EMAIL);

                if (email is null || !_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!code.Equals(correctCode))
                    return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

                var newUserModel = new UserModel { id = _userInfo.UserId, email = email };
                await _update.Update(newUserModel, null);
                await _tokenService.UpdateJwtToken();

                HttpContext.Session.Remove(_userInfo.UserId.ToString());
                HttpContext.Session.Remove(EMAIL);

                return StatusCode(201);
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(409, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}
