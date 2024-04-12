using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DTO;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/edit/password")]
    [ApiController]
    [Authorize]
    [EntityExceptionFilter]
    public class PasswordController(
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] IDataManagement dataManagament,
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
    }
}
