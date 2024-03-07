using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.DB;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/files")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<FileModel> _fileRepository;
        private readonly ISorting _sorting;
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;

        public FileController(
            IRepository<FileModel> fileRepository,
            ISorting sorting,
            IRedisCache redisCache,
            IUserInfo userInfo)
        {
            _fileRepository = fileRepository;
            _sorting = sorting;
            _redisCache = redisCache;
            _userInfo = userInfo;
        }

        #endregion

        [HttpDelete("{fileId}")]
        [XSRFProtection]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteFileFromHistory([FromRoute] int fileId)
        {
            try
            {
                await _fileRepository.DeleteByFilter(query => query.Where(f => f.file_id.Equals(fileId) && f.user_id.Equals(_userInfo.UserId)));
                await _redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{_userInfo.UserId}");

                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{fileId}")]
        [ProducesResponseType(typeof(FileModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetOneFile([FromRoute] int fileId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.FILES_PREFIX}{_userInfo.UserId}_{fileId}";

                var cache = await _redisCache.GetCachedData(cacheKey);
                if (cache is not null)
                    return StatusCode(200, new { file = JsonConvert.DeserializeObject<FileModel>(cache) });

                var file = await _fileRepository.GetByFilter(query => query.Where(f => f.file_id.Equals(fileId) && f.user_id.Equals(_userInfo.UserId)));
                if (file is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                await _redisCache.CacheData(cacheKey, file, TimeSpan.FromMinutes(5));

                return StatusCode(200, new { file });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
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
                var cacheKey = $"{ImmutableData.FILES_PREFIX}{_userInfo.UserId}_{skip}_{count}_{byDesc}_{type}_{category}_{mime}";

                var cacheFiles = await _redisCache.GetCachedData(cacheKey);
                if (cacheFiles is not null)
                    return StatusCode(200, new { files = JsonConvert.DeserializeObject<IEnumerable<FileModel>>(cacheFiles) });

                var files = await _fileRepository.GetAll(_sorting.SortFiles(_userInfo.UserId, skip, count, byDesc, type, mime, category));

                await _redisCache.CacheData(cacheKey, files, TimeSpan.FromMinutes(3));

                return StatusCode(200, new { files });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
