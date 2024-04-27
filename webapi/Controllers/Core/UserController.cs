using application.Abstractions.Endpoints.Core;
using application.Master_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/core/user")]
    [ApiController]
    [Authorize]
    public class UserController(
        IUserService service,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId, [FromQuery] bool own)
        {
            var response = await service.GetOne(userInfo.UserId, own ? userInfo.UserId : userId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { user = response.ObjectData });
        }

        [HttpGet("fully/{userId}")]
        public async Task<IActionResult> GetFullyUser([FromRoute] int userId, IFileService fileService,
            IStorageService storageService, IOfferService offerService)
        {
            var user = await service.GetOne(userInfo.UserId, userId);
            if (user.IsSuccess) return StatusCode(user.Status, new { message = user.Message });

            var files = await fileService.GetRange(userId, 0, 5, true, null, null);
            if (files.IsSuccess) return StatusCode(files.Status, new { message = files.Message });

            var storages = await storageService.GetRange(userId, 0, 5, true);
            if (storages.IsSuccess) return StatusCode(storages.Status, new { message = storages.Message });

            var offers = await offerService.GetRange(userId, 0, 5, true, true, null, null, true);
            if (offers.IsSuccess) return StatusCode(offers.Status, new { message = offers.Message });

            return StatusCode(200, new { user, files, storages, offers, isOwner = userInfo.UserId.Equals(userId) });
        }

        [HttpGet("range/{username}")]
        public async Task<IActionResult> GetRangeUsers([FromRoute] string username)
        {
            var response = await service.GetRange(username);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { users = response.ObjectData });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser()
        {
            var response = await service.DeleteOne(userInfo.UserId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
