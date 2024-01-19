using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using webapi.DB;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces.SQL;
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
        private readonly IRedisCache _redisCache;
        private readonly IUserInfo _userInfo;
        private readonly IDelete<FileModel> _deleteFileById;
        private readonly IRead<FileModel> _readFile;

        public FileController(
            IRedisCache redisCache,
            IUserInfo userInfo,
            IDelete<FileModel> deleteFileById,
            IRead<FileModel> readFile)
        {
            _redisCache = redisCache;
            _userInfo = userInfo;
            _deleteFileById = deleteFileById;
            _readFile = readFile;
        }

        [HttpDelete("{fileId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFileFromHistory([FromRoute] int fileId)
        {
            try
            {

                await _deleteFileById.DeleteById(fileId, _userInfo.UserId);
                HttpContext.Session.SetString(Constants.CACHE_FILES, true.ToString());

                return StatusCode(200, new { message = SuccessMessage.SuccessFileDeleted });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetOneFile([FromRoute] int fileId)
        {
            var cacheKey = $"File_{fileId}";

            try
            {
                var cacheFile = JsonConvert.DeserializeObject<FileModel>(await _redisCache.GetCachedData(cacheKey));
                if (cacheFile is not null)
                {
                    if (cacheFile.user_id != _userInfo.UserId)
                        return StatusCode(404);

                    return StatusCode(200, new { file = cacheFile });
                }

                var file = await _readFile.ReadById(fileId, null);

                if (file.user_id != _userInfo.UserId)
                    return StatusCode(404);

                await _redisCache.CacheData(cacheKey, file, TimeSpan.FromMinutes(5));

                return StatusCode(200, new { file });
            }
            catch (FileException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFiles([FromQuery] int skip, [FromQuery] int count)
        {
            var cacheKey = $"Files_{_userInfo.UserId}_{skip}_{count}";
            bool clearCache = HttpContext.Session.GetString(Constants.CACHE_FILES) is not null ? bool.Parse(HttpContext.Session.GetString(Constants.CACHE_FILES)) : true;

            if (clearCache)
            {
                await _redisCache.DeleteCache(cacheKey);
                HttpContext.Session.SetString(Constants.CACHE_FILES, false.ToString());
            }

            var cacheFiles = await _redisCache.GetCachedData(cacheKey);
            if (cacheFiles is not null)
                return StatusCode(200, new { files = JsonConvert.DeserializeObject<IEnumerable<FileModel>>(cacheFiles) });

            var files = await _readFile.ReadAll(_userInfo.UserId, skip, count);

            await _redisCache.CacheData(cacheKey, files, TimeSpan.FromMinutes(3));

            return StatusCode(200, new { files });
        }
    }
}
