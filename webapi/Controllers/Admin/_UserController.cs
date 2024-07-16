using application.Master_Services.Admin;
using data_access.Ef;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/user")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    public class _UserController(Admin_UserService service) : ControllerBase
    {
        [HttpDelete("{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            var response = await service.DeleteUser(userId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> BlockUser([FromRoute] int userId, [FromQuery] bool block)
        {
            var response = await service.BlockUser(userId, block);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpPost("seed")]
        [AllowAnonymous]
        public IActionResult Seed(ISeed seed)
        {
            return StatusCode(200, new { user = seed.AdminSeed() });
        }
    }
}
