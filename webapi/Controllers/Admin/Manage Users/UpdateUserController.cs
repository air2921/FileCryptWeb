using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Users
{
    [Route("api/admin/users/update")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class UpdateUserController : ControllerBase
    {
        private readonly IUpdate<UserModel> _update;
        private readonly IRead<UserModel> _read;

        public UpdateUserController(IUpdate<UserModel> update, IRead<UserModel> read)
        {
            _update = update;
            _read = read;
        }

        [HttpPut("role")]
        public async Task<IActionResult> UpdateRole(UserModel userModel)
        {
            try
            {
                var target = await _read.ReadById(userModel.id, null);
                if (target.role == Role.HighestAdmin.ToString())
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                if (userModel.role == Role.HighestAdmin.ToString())
                    return StatusCode(403);

                await _update.Update(userModel, null);
                return StatusCode(200, new { message = SuccessMessage.SuccessRoleUpdated });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
