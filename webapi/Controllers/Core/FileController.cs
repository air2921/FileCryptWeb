﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Core
{
    [Route("api/core/files")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
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

        [HttpDelete("{fileId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFileFromHistory([FromRoute] int fileId)
        {
            try
            {
                await _fileRepository.DeleteByFilter(query => query.Where(f => f.file_id.Equals(fileId) && f.user_id.Equals(_userInfo.UserId)));
                HttpContext.Session.SetString(Constants.CACHE_FILES, true.ToString());

                return StatusCode(200, new { message = SuccessMessage.SuccessFileDeleted });
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetOneFile([FromRoute] int fileId)
        {
            var cacheKey = $"File_{fileId}";

            var cacheFile = JsonConvert.DeserializeObject<FileModel>(await _redisCache.GetCachedData(cacheKey));
            if (cacheFile is not null)
            {
                if (cacheFile.user_id != _userInfo.UserId)
                    return StatusCode(404);

                return StatusCode(200, new { file = cacheFile });
            }

            var file = await _fileRepository.GetByFilter(query => query.Where(f => f.file_id.Equals(fileId) && f.user_id.Equals(_userInfo.UserId)));
            if (file is null)
                return StatusCode(404);

            await _redisCache.CacheData(cacheKey, file, TimeSpan.FromMinutes(5));

            return StatusCode(200, new { file });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFiles([FromQuery] int skip, [FromQuery] int count,
            [FromQuery] bool byDesc, [FromQuery] string? type, [FromQuery] string? mime)
        {
            var cacheKey = $"Files_{_userInfo.UserId}_{skip}_{count}_{byDesc}_{type}";

            bool clearCache = bool.TryParse(HttpContext.Session.GetString(Constants.CACHE_FILES), out var parsedValue) ? parsedValue : true;
            if (clearCache)
            {
                await _redisCache.DeleteCache(cacheKey);
                HttpContext.Session.SetString(Constants.CACHE_FILES, false.ToString());
            }

            var cacheFiles = await _redisCache.GetCachedData(cacheKey);
            if (cacheFiles is not null)
                return StatusCode(200, new { files = JsonConvert.DeserializeObject<IEnumerable<FileModel>>(cacheFiles) });

            var files = await _fileRepository.GetAll(_sorting.SortFiles(_userInfo.UserId, skip, count, byDesc, type, mime));

            await _redisCache.CacheData(cacheKey, files, TimeSpan.FromMinutes(3));

            return StatusCode(200, new { files });
        }
    }
}
