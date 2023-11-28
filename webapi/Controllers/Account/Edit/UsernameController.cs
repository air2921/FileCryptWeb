using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/username")]
    [ApiController]
    [Authorize]
    public class UsernameController : ControllerBase
    {
        private readonly IUpdate<UserModel> _update;
        private readonly IUserInfo _userInfo;
        private readonly ITokenService _tokenService;

        public UsernameController(
            IUpdate<UserModel> update,
            IUserInfo userInfo,
            ITokenService tokenService)
        {
            _update = update;
            _userInfo = userInfo;
            _tokenService = tokenService;
        }

        [HttpPut("new")]
        public async Task<IActionResult> UpdateUsername(UserModel userModel)
        {
            try
            {
                if (userModel.username.Length > 30)
                    return StatusCode(411, new { message = $"username {userModel.username} is too much large" });

                var newUserModel = new UserModel { id = _userInfo.UserId, username = userModel.username };

                await _update.Update(newUserModel, null);
                await _tokenService.UpdateJwtToken();

                return StatusCode(200, new { message = AccountSuccessMessage.UsernameUpdated, new_username = userModel.username });
            }
            catch (UserException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(404, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _tokenService.DeleteTokens();
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}
