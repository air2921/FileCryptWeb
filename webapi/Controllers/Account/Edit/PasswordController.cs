using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL.User;
using webapi.Localization.English;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/password")]
    [ApiController]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly IUpdateUser _update;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly FileCryptDbContext _dbContext;

        public PasswordController(
            IUpdateUser update,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IUserInfo userInfo,
            FileCryptDbContext dbContext)
        {
            _update = update;
            _passwordManager = passwordManager;
            _tokenService = tokenService;
            _userInfo = userInfo;
            _dbContext = dbContext;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> CheckPassword(UserModel userModel)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == _userInfo.Email);
            if (user is null)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });
            }

            bool IsCorrect = _passwordManager.CheckPassword(userModel.password_hash, user.password_hash);
            if (!IsCorrect)
                return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

            HttpContext.Session.SetString($"user({_userInfo.UserId}).ValidationCode", Guid.NewGuid().ToString()); 

            return StatusCode(307);
        }

        [HttpPut("new")]
        public async Task<IActionResult> UpdatePassword(UserModel userModel)
        {
            try
            {
                string? validateCode = HttpContext.Session.GetString($"user({_userInfo.UserId}).ValidationCode");

                if (string.IsNullOrWhiteSpace(validateCode))
                    return StatusCode(403, new { message = AccountErrorMessage.Forbidden });

                if (!Regex.IsMatch(userModel.password_hash, Validation.Password))
                    return StatusCode(422, new { message = AccountErrorMessage.InvalidFormatPassword });

                string password = _passwordManager.HashingPassword(userModel.password_hash);
                var newUserModel = new UserModel { id = _userInfo.UserId, password_hash = password };

                await _update.UpdatePasswordById(userModel);

                return StatusCode(200, new { message = AccountSuccessMessage.PasswordUpdated });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
