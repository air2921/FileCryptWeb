using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Helpers;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/files")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    [EntityExceptionFilter]
    public class Admin_FileController(IRepository<FileModel> fileRepository, IRedisCache redisCache) : ControllerBase
    {
        [HttpGet("{fileId}")]
        [ProducesResponseType(typeof(FileModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetFile([FromRoute] int fileId)
        {
            var file = await fileRepository.GetById(fileId);
            if (file is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return StatusCode(200, new { file });
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<FileModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetFiles([FromQuery] int? userId, [FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] string? category)
        {
            return StatusCode(200, new { files = await fileRepository
                    .GetAll(new FilesSortSpec(userId, skip, count, byDesc, null, null, category))});
        }

        [HttpDelete("{fileId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteFile([FromRoute] int fileId)
        {
            var deletedFile = await fileRepository.Delete(fileId);
            if (deletedFile is not null)
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{deletedFile.user_id}");

            return StatusCode(204);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> DeleteRangeFiles([FromBody] IEnumerable<int> identifiers)
        {
            var fileList = await fileRepository.DeleteMany(identifiers);
            await redisCache.DeleteRedisCache(fileList, ImmutableData.FILES_PREFIX, item => item.user_id);
            return StatusCode(204);
        }
    }
}
