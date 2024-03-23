using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/files")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin,Admin")]
    public class Admin_FileController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<FileModel> _fileRepository;
        private readonly ISorting _sorting;
        private readonly IRedisCache _redisCache;

        public Admin_FileController(IRepository<FileModel> fileRepository, ISorting sorting, IRedisCache redisCache)
        {
            _fileRepository = fileRepository;
            _sorting = sorting;
            _redisCache = redisCache;
        }

        #endregion

        [HttpGet("{fileId}")]
        [ProducesResponseType(typeof(FileModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetFile([FromRoute] int fileId)
        {
            try
            {
                var file = await _fileRepository.GetById(fileId);
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
                return StatusCode(200, new { files = await _fileRepository
                    .GetAll(_sorting.SortFiles(userId, skip, count, byDesc, null, null, category)) });
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
                var deletedFile = await _fileRepository.Delete(fileId);
                if(deletedFile is not null)
                    await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{deletedFile.user_id}");

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
                var fileList = await _fileRepository.DeleteMany(identifiers);
                await _redisCache.DeleteRedisCache(fileList, ImmutableData.FILES_PREFIX, item => item.user_id);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
