﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/username")]
    [ApiController]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class UsernameController : ControllerBase
    {
        private readonly IUpdate<UserModel> _update;
        private readonly ILogger<UsernameController> _logger;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;

        public UsernameController(
            IUpdate<UserModel> update,
            ILogger<UsernameController> logger,
            IUserInfo userInfo,
            ITokenService tokenService)
        {
            _update = update;
            _logger = logger;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateUsername([FromQuery] string username)
        {
            try
            {
                if (username.Length > 30)
                    return StatusCode(411, new { message = $"username {username} is too much large" });

                var newUserModel = new UserModel { id = _userInfo.UserId, username = username };

                await _update.Update(newUserModel, null);
                _logger.LogInformation($"username was updated in db. {username}#{_userInfo.UserId}");

                await _tokenService.UpdateJwtToken();
                _logger.LogInformation("jwt with a new claims was updated");

                return StatusCode(200, new { message = AccountSuccessMessage.UsernameUpdated });
            }
            catch (UserException ex)
            {
                _logger.LogWarning($"Non-existent user {_userInfo.Username}#{_userInfo.UserId} was requested to authorized endpoint.\nTrying delete tokens from cookie");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(409, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Error when trying to update jwt.\nTrying delete tokens");
                _tokenService.DeleteTokens();
                _logger.LogWarning("Tokens was deleted");
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}
