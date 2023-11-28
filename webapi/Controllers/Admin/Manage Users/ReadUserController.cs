using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.Controllers.Admin.Manage_Users
{
    [Route("api/admin/users/get")]
    [ApiController]
    public class ReadUserController : ControllerBase
    {
        private readonly IRead<UserModel> _read;

        public ReadUserController(IRead<UserModel> read)
        {
            _read = read;
        }

        [HttpGet("one")]
        public async Task<IActionResult> ReadUser(int id)
        {
            try
            {
                var user = await _read.ReadById(id, null);

                return StatusCode(200, new { user });
            }
            catch (UserException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ReadAllUsers()
        {
            try
            {
                var users = await _read.ReadAll();

                return StatusCode(200, new { users });
            }
            catch (UserException ex)
            {
                return StatusCode(200, new { message = ex.Message });
            }
        }
    }
}
