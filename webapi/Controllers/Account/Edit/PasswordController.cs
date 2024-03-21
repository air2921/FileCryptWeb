using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/password")]
    [ApiController]
    [Authorize]
    public class PasswordController(
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] IDataManagament dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IPasswordManager passwordManager,
        IUserInfo userInfo) : ControllerBase
    {

        [HttpPut]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 422)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordDTO passwordDto)
        {
            try
            {
                if (!validator.IsValid(passwordDto.NewPassword))
                    return StatusCode(422, new { message = Message.INVALID_FORMAT });

                var user = await userRepository.GetById(userInfo.UserId);
                if (user is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!passwordManager.CheckPassword(passwordDto.OldPassword, user.password))
                    return StatusCode(401, new { message = Message.INCORRECT });

                await transaction.CreateTransaction(user, passwordDto.NewPassword);
                await dataManagament.DeleteData(userInfo.UserId);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
