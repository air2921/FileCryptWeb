using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Users
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    [ValidateAntiForgeryToken]
    public class UpdateUserController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<UpdateUserController> _logger;
        private readonly IUpdate<UserModel> _updateUser;
        public readonly IUpdate<TokenModel> _updateToken;
        private readonly IRead<UserModel> _read;

        public UpdateUserController(
            IUserInfo userInfo,
            ILogger<UpdateUserController> logger,
            IUpdate<UserModel> updateUser,
            IUpdate<TokenModel> updateToken,
            IRead<UserModel> read)
        {
            _userInfo = userInfo;
            _logger = logger;
            _updateUser = updateUser;
            _updateToken = updateToken;
            _read = read;
        }

        [HttpPut("role")]
        public async Task<IActionResult> UpdateRole([FromBody] UserModel userModel)
        {
            try
            {
                var target = await _read.ReadById(userModel.id, null);
                if (target.role == Role.HighestAdmin.ToString())
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                if (userModel.role == Role.HighestAdmin.ToString())
                    return StatusCode(403);

                await _updateUser.Update(userModel, null);
                _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} updated role user#{userModel.id}");

                return StatusCode(200, new { message = SuccessMessage.SuccessRoleUpdated });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpPut("block")]
        public async Task<IActionResult> BlockUser([FromQuery] int userId, [FromQuery] bool block)
        {
            try
            {
                var targetUser = await _read.ReadById(userId, null);

                if (_userInfo.Role != Role.HighestAdmin.ToString() && targetUser.role == Role.HighestAdmin.ToString())
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                targetUser.is_blocked = block;

                await _updateUser.Update(targetUser, null);

                if (block)
                {
                    var tokenModel = new TokenModel
                    {
                        user_id = userId,
                        refresh_token = Guid.NewGuid().ToString(),
                        expiry_date = DateTime.UtcNow.AddYears(-100)
                    };

                    await _updateToken.Update(tokenModel, true);
                    return StatusCode(200, new { message = SuccessMessage.UserBlocked });
                }

                return StatusCode(200, new { message = SuccessMessage.UserUnlocked });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (TokenException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
