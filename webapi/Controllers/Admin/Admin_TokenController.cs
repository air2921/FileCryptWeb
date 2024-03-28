using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces;
using webapi.Localization;
using webapi.Models;
using webapi.Exceptions;
using webapi.Interfaces.Services;
using webapi.Interfaces.Controllers.Services;
using webapi.Helpers;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/tokens")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
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
            try
            {
                var target = await userRepository.GetById(userId);
                if (target is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!validator.IsValid(target.role, userInfo.Role))
                    return StatusCode(403, new { message = Message.FORBIDDEN });

                await transaction.CreateTransaction(null, target.id);
                return StatusCode(200, new { message = Message.REMOVED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("revoke/{tokenId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeToken(int tokenId)
        {
            try
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
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
