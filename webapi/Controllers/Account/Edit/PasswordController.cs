using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/password")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class PasswordController : ControllerBase
    {
        private readonly IUpdate<UserModel> _update;
        private readonly ILogger<PasswordController> _logger;
        private readonly IPasswordManager _passwordManager;
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        private readonly FileCryptDbContext _dbContext;

        public PasswordController(
            IUpdate<UserModel> update,
            ILogger<PasswordController> logger,
            IPasswordManager passwordManager,
            ITokenService tokenService,
            IUserInfo userInfo,
            FileCryptDbContext dbContext)
        {
            _update = update;
            _logger = logger;
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
                _logger.LogWarning($"Non-existent user {_userInfo.Username}#{_userInfo.UserId} was requested to authorized endpoint.\nTrying delete tokens from cookie");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(404, new { message = AccountErrorMessage.UserNotFound });
            }

            bool IsCorrect = _passwordManager.CheckPassword(userModel.password_hash, user.password_hash);
            if (!IsCorrect)
                return StatusCode(401, new { message = AccountErrorMessage.PasswordIncorrect });

            _logger.LogInformation($"{_userInfo.Username}#{_userInfo.UserId} Password is correct, action is allowed");
            return StatusCode(200);
        }

        [HttpPut("new")]
        public async Task<IActionResult> UpdatePassword(UserModel userModel)
        {
            try
            {
                if (!Regex.IsMatch(userModel.password_hash, Validation.Password))
                    return StatusCode(422, new { message = AccountErrorMessage.InvalidFormatPassword });

                var newUserModel = new UserModel { id = _userInfo.UserId, password_hash = _passwordManager.HashingPassword(userModel.password_hash) };
                await _update.Update(userModel, null);

                _logger.LogInformation("Password was hashed andd updated in db");
                return StatusCode(200, new { message = AccountSuccessMessage.PasswordUpdated });
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"Non-existent user {_userInfo.Username}#{_userInfo.UserId} was requested to authorized endpoint.\nTrying delete tokens from cookie");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
