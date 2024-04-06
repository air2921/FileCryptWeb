using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/files")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_FileController(IRepository<FileModel> fileRepository, ISorting sorting, IRedisCache redisCache) : ControllerBase
    {
        [HttpGet("{fileId}")]
        [ProducesResponseType(typeof(FileModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetFile([FromRoute] int fileId)
        {
            try
            {
                var file = await fileRepository.GetById(fileId);
                if (file is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { file });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<FileModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetFiles([FromQuery] int? userId, [FromQuery] int? skip, [FromQuery] int? count,
            [FromQuery] bool byDesc, [FromQuery] string? category)
        {
            try
            {
                return StatusCode(200, new { files = await fileRepository
                    .GetAll(sorting.SortFiles(userId, skip, count, byDesc, null, null, category)) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{fileId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteFile([FromRoute] int fileId)
        {
            try
            {
                var deletedFile = await fileRepository.Delete(fileId);
                if(deletedFile is not null)
                    await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{deletedFile.user_id}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> DeleteRangeFiles([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                var fileList = await fileRepository.DeleteMany(identifiers);
                await redisCache.DeleteRedisCache(fileList, ImmutableData.FILES_PREFIX, item => item.user_id);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
