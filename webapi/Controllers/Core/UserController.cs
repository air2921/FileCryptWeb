using application.Master_Services.Core;
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
        UsersService service,
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
        public async Task<IActionResult> GetFullyUser([FromRoute] int userId, FilesService fileService,
            StoragesService storageService, OfferService offerService)
        {
            var user = await service.GetOne(userInfo.UserId, userId);
            if (!user.IsSuccess) return StatusCode(user.Status, new { message = user.Message });

            var files = await fileService.GetRange(userId, 0, 5, true, null, null);
            if (!files.IsSuccess) return StatusCode(files.Status, new { message = files.Message });

            var storages = await storageService.GetRange(userId, 0, 5, true);
            if (!storages.IsSuccess) return StatusCode(storages.Status, new { message = storages.Message });

            var offers = await offerService.GetRange(userId, 0, 5, true, true, null, null, true);
            if (!offers.IsSuccess) return StatusCode(offers.Status, new { message = offers.Message });

            return StatusCode(200, new 
            {
                user = user.ObjectData,
                isOwner = userInfo.UserId.Equals(userId),
                files = files.ObjectData,
                storages = storages.ObjectData,
                offers = offers.ObjectData,
            });
        }

        [HttpGet("range/{username}")]
        public async Task<IActionResult> GetRangeUsers([FromRoute] string username,
            [FromQuery] int skip, [FromQuery] int count)
        {
            var response = await service.GetRange(username, skip, count);
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
