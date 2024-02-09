using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.Interfaces;
using webapi.Interfaces.Services;
using webapi.Localization.Exceptions;
using webapi.Localization;
using webapi.Models;
using webapi.Exceptions;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/tokens")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public class Admin_TokenController : ControllerBase
    {
        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly IRepository<UserModel> _userRepository;
        private readonly IUserInfo _userInfo;

        public Admin_TokenController(IRepository<TokenModel> tokenRepository, IRepository<UserModel> userRepository, IUserInfo userInfo)
        {
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
            _userInfo = userInfo;
        }

        [HttpPut("revoke/userId")]
        public async Task<IActionResult> RevokeToken([FromRoute] int userId)
        {
            try
            {
                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404);

                if (!User.IsInRole("HighestAdmin") && target.role.Equals("HighestAdmin"))
                    return StatusCode(403);

                var targetToken = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(userId)));
                if (targetToken is null)
                    return StatusCode(403);

                targetToken.refresh_token = Guid.NewGuid().ToString();
                targetToken.expiry_date = DateTime.UtcNow.AddYears(-100);

                await _tokenRepository.Update(targetToken);

                return StatusCode(200, new { message = SuccessMessage.SuccessRefreshRevoked });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
