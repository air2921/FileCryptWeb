using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [EntityExceptionFilter]
    public class Admin_UserController(
        [FromKeyedServices(ImplementationKey.ADMIN_USER_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ADMIN_USER_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository) : ControllerBase
    {
        [HttpGet("{userId}")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetUser([FromRoute] int userId)
        {
            var user = await userRepository.GetById(userId);
            if (user is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return StatusCode(200, new { user });
        }

        [HttpDelete("{userId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            var target = await userRepository.GetById(userId);
            if (target is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (!validator.IsValid(target.role))
                return StatusCode(403, new { message = Message.FORBIDDEN });

            await userRepository.Delete(userId);
            return StatusCode(204);
        }

        [HttpPut("block/{userId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> BlockUser([FromRoute] int userId, [FromQuery] bool block)
        {
            var target = await userRepository.GetById(userId);
            if (target is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (!validator.IsValid(target.role))
                return StatusCode(403, new { message = Message.FORBIDDEN });

            await transaction.CreateTransaction(target, block);
            return StatusCode(200, new { message = Message.UPDATED });
        }

        [HttpPut("role/{userId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UpdateRole([FromRoute] int userId, [FromQuery] string role)
        {
            var target = await userRepository.GetById(userId);
            if (target is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (!validator.IsValid(target.role))
                return StatusCode(403, new { message = Message.FORBIDDEN });

            target.role = role;
            await userRepository.Update(target);

            return StatusCode(200, new { message = Message.UPDATED });
        }
    }
}
