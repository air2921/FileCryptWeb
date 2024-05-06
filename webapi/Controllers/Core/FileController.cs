using application.Abstractions.Endpoints.Core;
using application.DTO.Outer;
using application.Helpers.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Helpers.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/core/file")]
    [ApiController]
    [Authorize]
    public class FileController(
        ICryptographyService cryptographyService,
        IFileService fileService,
        IUserInfo userInfo) : ControllerBase
    {
        [HttpPost("cypher/{storageId}/{keyId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CypherFile(IFormFile file, [FromRoute] int storageId,
            [FromRoute] int keyId, [FromQuery] bool encrypt, [FromQuery] int code)
        {
            var response = await cryptographyService.Cypher(new CypherFileDTO
            {
                Operation = encrypt ? "encrypt" : "decrypt",
                StorageId = storageId,
                KeyId = keyId,
                Code = code.ToString(),
                UserId = userInfo.UserId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = file.OpenReadStream()
            });

            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });

            var stream = GetStream(response.ObjectData);
            if (stream is null)
                return StatusCode(500, new { message = Message.ERROR });

            return File(stream, file.ContentType, file.FileName);
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile([FromRoute] int fileId)
        {
            var response = await fileService.GetOne(userInfo.UserId, fileId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { file = response.ObjectData });
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRangeFiles([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] string? category, [FromQuery] string? mime)
        {
            var response = await fileService.GetRange(userInfo.UserId, skip, count, byDesc, category, mime);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status, new { files = response.ObjectData });
        }

        [HttpDelete("{fileId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile([FromRoute] int fileId)
        {
            var response = await fileService.DeleteOne(userInfo.UserId, fileId);
            if (!response.IsSuccess)
                return StatusCode(response.Status, new { message = response.Message });
            else
                return StatusCode(response.Status);
        }

        private Stream? GetStream(object? response)
        {
            if (response is not Stream stream)
                return null;

            return stream;
        }
    }
}
