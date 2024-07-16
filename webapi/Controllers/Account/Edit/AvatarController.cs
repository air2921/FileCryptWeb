using application.DTO.Outer;
using application.Helpers.Localization;
using application.Master_Services.Account.Edit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Account.Edit
{
    [Route("api/account/avatar")]
    [ApiController]
    [Authorize]
    public class AvatarController(
        AvatarService service,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAvatar([FromRoute] int userId)
        {
            var response = await service.Download(userId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });

            var file = GetDTO(response.ObjectData);
            if (file is null)
                return StatusCode(500, new { message = Message.ERROR });
            else
                return File(file.AvatarContent, file.ContentType, file.Name);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAvatar(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return StatusCode(400, new { message = "Invalid file" });

            var response = await service.Change(
                file.OpenReadStream(), file.FileName, file.ContentType, userInfo.UserId, Guid.NewGuid().ToString());

            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvatar()
        {
            var response = await service.Delete(userInfo.UserId);
            return StatusCode(response.Status, response.IsSuccess ? null : response.Message);
        }

        private AvatarDTO? GetDTO(object? response)
        {
            if (response is not AvatarDTO dto)
                return null;

            return dto;
        }
    }
}
