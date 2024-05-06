using application.Abstractions.Endpoints.Core;
using application.DTO.Outer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/core/storage")]
    [ApiController]
    [Authorize]
    public class KeyStorageController(
        IStorageService storageService,
        IStorageItemService storageItemService,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStorage([FromBody] StorageDTO dto)
        {
            var response = await storageService.Add(dto.Name, dto.Code.ToString(), dto.Description, userInfo.UserId);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpGet("{storageId}")]
        public async Task<IActionResult> GetStorage([FromRoute] int storageId)
        {
            var response = await storageService.GetOne(storageId, userInfo.UserId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { storage = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeStorages([FromQuery] int skip,
            [FromQuery] int count, [FromQuery] bool byDesc)
        {
            var response = await storageService.GetRange(userInfo.UserId, skip, count, byDesc);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { storages = response.ObjectData });
        }

        [HttpDelete("{storageId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStorage([FromRoute] int storageId, [FromQuery] int code)
        {
            var response = await storageService.DeleteOne(userInfo.UserId, storageId, code.ToString());
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        [HttpPost("{storageId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([FromRoute] int storageId, [FromQuery] string name,
            [FromQuery] string value, [FromQuery] int code)
        {
            var response = await storageItemService.Add(userInfo.UserId, storageId, code.ToString(), name, value);
            return StatusCode(response.Status, new { message = response.Message });
        }

        [HttpGet("{storageId}/{keyId}")]
        public async Task<IActionResult> GetKey([FromRoute] int storageId, [FromRoute] int keyId,
            [FromQuery] int code)
        {
            var response = await storageItemService.GetOne(userInfo.UserId, storageId, keyId, code.ToString());
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { key = response.ObjectData });
        }

        [HttpGet("range/{storageId}")]
        public async Task<IActionResult> GetRangeKeys([FromRoute] int storageId, [FromQuery] int skip,
            [FromQuery] int count, [FromQuery] bool byDesc, [FromQuery] int code)
        {
            var response = await storageItemService.GetRange(userInfo.UserId, storageId, skip, count, byDesc, code.ToString());
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { keys = response.ObjectData });
        }

        [HttpDelete("{storageId}/{keyId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteKey([FromRoute] int storageId,
            [FromRoute] int keyId, [FromQuery] int code)
        {
            var response = await storageItemService.DeleteOne(userInfo.UserId, storageId, keyId, code.ToString());
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }
    }
}
