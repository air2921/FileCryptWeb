using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    public class Admin_UserController : ControllerBase
    {
        private readonly IRepository<UserModel> _userRepository;
        private readonly IRepository<TokenModel> _tokenRepository;

        public Admin_UserController(IRepository<UserModel> userRepository, IRepository<TokenModel> tokenRepository)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        public async Task<IActionResult> GetUser([FromRoute] int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null)
                return StatusCode(404);

            return StatusCode(200, new { user });
        }

        [HttpDelete("{userId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            try
            {
                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404);

                if (target.role.Equals("HighestAdmin"))
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                await _userRepository.Delete(userId);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("block/{userId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> BlockUser([FromRoute] int userId, [FromQuery] bool block)
        {
            try
            {
                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404);

                if (target.role.Equals("HighestAdmin"))
                    return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

                target.is_blocked = block;
                await _userRepository.Update(target);

                if (block)
                {
                    var targetToken = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(userId)));
                    if (targetToken is null)
                        return StatusCode(404);

                    targetToken.refresh_token = Guid.NewGuid().ToString();
                    targetToken.expiry_date = DateTime.UtcNow.AddYears(-100);

                    await _tokenRepository.Update(targetToken);
                }

                return StatusCode(200);
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("role/{userId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> UpdateRole([FromRoute] int userId, [FromQuery] string role)
        {
            if (role.Equals("HighestAdmin"))
                return StatusCode(403);

            var target = await _userRepository.GetById(userId);
            if (target is null)
                return StatusCode(404);

            if (target.role.Equals("HighestAdmin"))
                return StatusCode(403, new { message = ErrorMessage.HighestRoleError });

            target.role = role;
            await _userRepository.Update(target);

            return StatusCode(204);
        }
    }
}
