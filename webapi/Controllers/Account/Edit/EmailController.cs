using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/email")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly FileCryptDbContext _dbContext;
        private readonly IUpdate<UserModel> _update;
        private readonly IEmailSender<UserModel> _email;
        private readonly IPasswordManager _passwordManager;
        private readonly IGenerateSixDigitCode _generateCode;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly IValidation _validation;
        private readonly IMemoryCache _memoryCache;

        public EmailController(
            FileCryptDbContext dbContext,
            IUpdate<UserModel> update,
            IEmailSender<UserModel> email,
            IPasswordManager passwordManager,
            IGenerateSixDigitCode generateCode,
            ITokenService tokenService,
            IUserInfo userInfo,
            IValidation validation,
            IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _update = update;
            _email = email;
            _passwordManager = passwordManager;
            _generateCode = generateCode;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _validation = validation;
            _memoryCache = memoryCache;
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
            string message = EmailMessage.ConfirmOldEmailBody;

            var newUserModel = new UserModel { username = _userInfo.Username, email = _userInfo.Email };
            await _email.SendMessage(newUserModel, messageHeader, message);

            HttpContext.Session.SetString(_userInfo.Email, code.ToString());

            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
        }

        [HttpPost("confirm/old")]
        public IActionResult ConfirmOldEmail([FromBody] int verifyCode)
        {
            try
            {
                string? code = HttpContext.Session.GetString(_userInfo.Email);
                if (code is null)
                    return StatusCode(500, new { message = AccountErrorMessage.VerifyCodeNull });

                int correctCode = int.Parse(code);

                if (!_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (verifyCode.Equals(correctCode))
                    return StatusCode(307, new { message = AccountSuccessMessage.OldEmailConfirmed });

                return StatusCode(401, new { message = AccountErrorMessage.CodeIncorrect });
            }
            catch (ArgumentNullException)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("new")]
        public async Task<IActionResult> SendEmailVerificationCode([FromBody] UserModel userModel)
        {
            string email = userModel.email.ToLowerInvariant();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
            if (user is not null)
                return StatusCode(409, new { message = AccountErrorMessage.UserExists });

            _memoryCache.Set("Email", email);

            int code = _generateCode.GenerateSixDigitCode();
            string messageHeader = EmailMessage.ConfirmNewEmailHeader;
            string message = EmailMessage.ConfirmNewEmailBody;
            var newUserModel = new UserModel { username = _userInfo.Username, email = email };

            HttpContext.Session.SetString(_userInfo.UserId.ToString(), code.ToString());

            await _email.SendMessage(newUserModel, messageHeader, message);

            return StatusCode(200, new { message = AccountSuccessMessage.EmailSended });
        }

        [HttpPut("confirm/new")]
        public async Task<IActionResult> ConfirmAndUpdateNewEmail([FromBody] int verifyCode)
        {
            try
            {
                string? code = HttpContext.Session.GetString(_userInfo.UserId.ToString());
                string? email = _memoryCache.Get<string>("Email");
                if (code is null || email is null)
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                int correctCode = int.Parse(code);

                if (!_validation.IsSixDigit(correctCode))
                    return StatusCode(500, new { message = AccountErrorMessage.Error });

                if (!verifyCode.Equals(correctCode))
                    return StatusCode(422, new { message = AccountErrorMessage.CodeIncorrect });

                var newUserModel = new UserModel { id = _userInfo.UserId, email = email };
                await _update.Update(newUserModel, null);
                await _tokenService.UpdateJwtToken();

                _memoryCache.Remove("Email");

                return StatusCode(200);
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
