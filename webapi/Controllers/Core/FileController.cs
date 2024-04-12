using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Abstractions;
using webapi.Services.Core.Data_Handlers;

namespace webapi.Controllers.Core
{
    [Route("api/core/files")]
    [ApiController]
    [Authorize]
    [EntityExceptionFilter]
    public class FileController(
        IRepository<FileModel> fileRepository,
        IRedisCache redisCache,
        IUserInfo userInfo,
        ICacheHandler<FileModel> cache) : ControllerBase
    {
        [HttpDelete("{fileId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteFileFromHistory([FromRoute] int fileId)
        {
            var file = await fileRepository.DeleteByFilter(new FileByIdAndRelationSpec(fileId, userInfo.UserId));
            if (file is not null)
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{userInfo.UserId}");

            return StatusCode(204);
        }

        [HttpGet("{fileId}")]
        [ProducesResponseType(typeof(FileModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetOneFile([FromRoute] int fileId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.FILES_PREFIX}{userInfo.UserId}_{fileId}";
                var file = await cache.CacheAndGet(new FileObject(cacheKey, userInfo.UserId, fileId));
                if (file is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { file });
            }
            catch (FormatException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<FileModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAllFiles([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] string? type, [FromQuery] string? category, [FromQuery] string? mime)
        {
            try
            {
                var cacheKey = $"{ImmutableData.FILES_PREFIX}{userInfo.UserId}_{skip}_{count}_{byDesc}_{type}_{category}_{mime}";
                return StatusCode(200, new { files = await cache.CacheAndGetRange(new FileRangeObject(cacheKey, userInfo.UserId, skip, count, byDesc, type, mime, category)) });
            }
            catch (FormatException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }
    }
}
