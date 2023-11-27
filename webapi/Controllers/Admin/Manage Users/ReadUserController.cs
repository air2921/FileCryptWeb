using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.SQL.User;

namespace webapi.Controllers.Admin.Manage_Users
{
    [Route("api/admin/users/get")]
    [ApiController]
    public class ReadUserController : ControllerBase
    {
        private readonly IReadUser _readUser;

        public ReadUserController(IReadUser readUser)
        {
            _readUser = readUser;
        }

        [HttpGet("one")]
        public async Task<IActionResult> ReadUser(int id)
        {
            try
            {
                var user = await _readUser.ReadFullUser(id);

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
                var users = await _readUser.ReadAllUsers();

                return StatusCode(200, new { users });
            }
            catch (UserException ex)
            {
                return StatusCode(200, new { message = ex.Message });
            }
        }
    }
}
