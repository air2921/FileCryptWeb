using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
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
        private readonly IUpdate<UserModel> _update;
        private readonly IRead<UserModel> _read;
        private readonly IDelete<UserModel> _delete;

        public DeleteUserController(IUpdate<UserModel> update, IRead<UserModel> read, IDelete<UserModel> delete)
        {
            _update = update;
            _read = read;
            _delete = delete;
        }

        [HttpDelete("one")]
        public async Task<IActionResult> DeleteUser([FromBody] int id)
        {
            try
            {
                var target = await _read.ReadById(id, null);

                if (User.IsInRole(Role.HighestAdmin.ToString()))
                {
                    if (target.role == Role.HighestAdmin.ToString())
                        return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                    await _delete.DeleteById(id);
                    return StatusCode(204, new { message = SuccessMessage.SuccessDeletedUser });
                }
                else
                {
                    if (target.role == Role.Admin.ToString() || target.role == Role.HighestAdmin.ToString())
                        return StatusCode(403, new { message = ErrorMessage.AdminCannotDelete });

                    await _delete.DeleteById(id);
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
