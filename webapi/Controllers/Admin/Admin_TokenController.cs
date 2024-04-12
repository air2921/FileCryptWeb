using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Localization;
using webapi.Models;
using webapi.Helpers;
using webapi.DB.Abstractions;
using webapi.Services.Abstractions;
using webapi.Helpers.Abstractions;
using webapi.Attributes;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/tokens")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [EntityExceptionFilter]
    public class Admin_TokenController(
        [FromKeyedServices(ImplementationKey.ADMIN_TOKEN_SERVICE)] ITransaction<TokenModel> transaction,
        [FromKeyedServices(ImplementationKey.ADMIN_TOKEN_SERVICE)] IValidator validator,
        IRepository<TokenModel> tokenRepository,
        IRepository<UserModel> userRepository,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpDelete("revoke/all/{userId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> RevokeAllUserTokens([FromRoute] int userId)
        {
            var target = await userRepository.GetById(userId);
            if (target is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (!validator.IsValid(target.role, userInfo.Role))
                return StatusCode(403, new { message = Message.FORBIDDEN });

            await transaction.CreateTransaction(null, target.id);
            return StatusCode(200, new { message = Message.REMOVED });
        }

        [HttpDelete("revoke/{tokenId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeToken(int tokenId)
        {
            var token = await tokenRepository.GetById(tokenId);
            if (token is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            var target = await userRepository.GetById(token.user_id);
            if (target is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (!validator.IsValid(target.role, userInfo.Role))
                return StatusCode(403, new { message = Message.FORBIDDEN });

            await tokenRepository.Delete(tokenId);
            return StatusCode(200, new { message = Message.REMOVED });
        }
    }
}
