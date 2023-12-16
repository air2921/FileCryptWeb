using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using webapi.Controllers.Admin.Manage_Notifications;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Users
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class DeleteUserController : ControllerBase
    {
        private readonly IUserInfo _userInfo;
        private readonly ILogger<DeleteUserController> _logger;
        private readonly IRead<UserModel> _read;
        private readonly IDelete<UserModel> _delete;

        public DeleteUserController(IUserInfo userInfo, ILogger<DeleteUserController> logger, IRead<UserModel> read, IDelete<UserModel> delete)
        {
            _userInfo = userInfo;
            _logger = logger;
            _read = read;
            _delete = delete;
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            try
            {
                var target = await _read.ReadById(userId, null);

                if (User.IsInRole(Role.HighestAdmin.ToString()))
                {
                    if (target.role == Role.HighestAdmin.ToString())
                        return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                    await _delete.DeleteById(userId);
                    _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} deleted user#{userId}");

                    return StatusCode(204, new { message = SuccessMessage.SuccessDeletedUser });
                }
                else
                {
                    if (target.role == Role.Admin.ToString() || target.role == Role.HighestAdmin.ToString())
                        return StatusCode(403, new { message = ErrorMessage.AdminCannotDelete });

                    await _delete.DeleteById(userId);
                    _logger.LogWarning($"{_userInfo.Username}#{_userInfo.UserId} deleted user#{userId}");

                    return StatusCode(200, new { message = SuccessMessage.SuccessDeletedUser });
                }
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
