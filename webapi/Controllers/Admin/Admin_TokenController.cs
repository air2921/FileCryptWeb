using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces;
using webapi.Localization;
using webapi.Models;
using webapi.Exceptions;
using webapi.Attributes;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/tokens")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [ValidateAntiForgeryToken]
    public class Admin_TokenController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<TokenModel> _tokenRepository;
        private readonly IRepository<UserModel> _userRepository;

        public Admin_TokenController(IRepository<TokenModel> tokenRepository, IRepository<UserModel> userRepository)
        {
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
        }

        #endregion

        [HttpPut("revoke/{userId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> RevokeToken([FromRoute] int userId)
        {
            try
            {
                var target = await _userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!User.IsInRole("HighestAdmin") && target.role.Equals("HighestAdmin"))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                var targetToken = await _tokenRepository.GetByFilter(query => query.Where(t => t.user_id.Equals(userId)));
                if (targetToken is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await DbUpdate(targetToken);
                return StatusCode(200, new { message = Message.REMOVED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Helper]
        private async Task DbUpdate(TokenModel tokenModel)
        {
            try
            {
                tokenModel.refresh_token = Guid.NewGuid().ToString();
                tokenModel.expiry_date = DateTime.UtcNow.AddYears(-100);

                await _tokenRepository.Update(tokenModel);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
        }
    }
}
