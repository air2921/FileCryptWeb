using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/username")]
    [ApiController]
    [Authorize]
    public class UsernameController(
        [FromKeyedServices(ImplementationKey.ACCOUNT_USERNAME_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_USERNAME_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_USERNAME_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IUserInfo userInfo,
        ITokenService tokenService) : ControllerBase
    {
        [HttpPut]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        [ProducesResponseType(typeof(object), 206)]
        public async Task<IActionResult> UpdateUsername([FromQuery] string username)
        {
            try
            {
                if (!validator.IsValid(username))
                    return StatusCode(400, new { message = Message.INVALID_FORMAT });

                var user = await userRepository.GetById(userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await transaction.CreateTransaction(user, username);
                await tokenService.UpdateJwtToken();
                await dataManagament.DeleteData(userInfo.UserId);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                tokenService.DeleteTokens();
                return StatusCode(206, new { message = ex.Message });
            }
        }
    }
}
