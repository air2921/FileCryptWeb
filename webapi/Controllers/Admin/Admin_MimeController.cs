using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces;
using webapi.Localization;
using webapi.Models;
using Microsoft.AspNetCore.Authorization;
using webapi.Helpers;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/mime")]
    [ApiController]
    public class Admin_MimeController : ControllerBase
    {
        private readonly IRepository<FileMimeModel> _mimeRepository;
        private readonly ILogger<Admin_MimeController> _logger;
        private readonly IRedisCache _redisCache;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileManager _fileManager;

        public Admin_MimeController(
            IRepository<FileMimeModel> mimeRepository,
            ILogger<Admin_MimeController> logger,
            IRedisCache redisCache,
            IWebHostEnvironment webHostEnvironment,
            IFileManager fileManager)
        {
            _mimeRepository = mimeRepository;
            _redisCache = redisCache;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _fileManager = fileManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> CreateNewMime([FromQuery] string mime)
        {
            try
            {
                await _mimeRepository.Add(new FileMimeModel { mime_name = mime });
                await _redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                _logger.LogWarning($"new MIME type: {mime}. Added in db");

                return StatusCode(201, new { message = SuccessMessage.SuccessCreatedNewMime });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("many")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        public async Task<IActionResult> CreateSecureBaseMIMICollection([FromQuery] bool secured)
        {
            var mimeModels = new HashSet<FileMimeModel>();

            try
            {
                if (secured)
                {
                    string[] baseMimes =
                    {
                        "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp", "image/svg+xml", "application/pdf",
                        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        "application/vnd.ms-powerpoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "audio/mpeg",
                        "audio/wav", "audio/mp3", "video/mp4", "video/mpeg", "video/webm", "video/mkv", "video/x-matroska", "application/zip",
                        "application/x-rar-compressed", "application/x-tar", "application/x-7z-compressed", "text/plain",
                        "text/html", "text/css", "text/xml", "application/json", "application/rtf", "text/richtext",
                        "font/woff", "font/woff2", "font/otf", "font/ttf"
                    };

                    foreach (string mime in baseMimes)
                    {
                        mimeModels.Add(new FileMimeModel { mime_name = mime });
                    }

                    await _mimeRepository.AddRange(mimeModels);
                }
                else
                {
                    var basePath = Path.Combine(_webHostEnvironment.ContentRootPath, "..", "data");
                    string[] dataFiles = Directory.GetFiles(basePath);

                    var allMimes = new HashSet<string>();

                    foreach (var dataFile in dataFiles)
                    {
                        allMimes.UnionWith(_fileManager.GetMimesFromCsvFile(dataFile));
                    }

                    var existingMimes = await _mimeRepository.GetAll();
                    var HashsetMimes = existingMimes.Select(m => m.mime_name).ToHashSet();

                    allMimes.UnionWith(HashsetMimes);

                    foreach (var newMime in allMimes)
                    {
                        mimeModels.Add(new FileMimeModel { mime_name = newMime });
                    }

                    await _mimeRepository.AddRange(mimeModels);
                    await _redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                }

                return StatusCode(201, new { message = SuccessMessage.SuccessMimeCollectionCreate });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            finally
            {
                await _redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
            }
        }

        [HttpGet("mimeId")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        public async Task<IActionResult> GetMime([FromRoute] int mimeId)
        {
            try
            {
                var mime = await _mimeRepository.GetById(mimeId);
                if (mime is null)
                    return StatusCode(404);

                return StatusCode(200, new { mime });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        public async Task<IActionResult> GetAllMime([FromQuery] int? skip, [FromQuery] int? count)
        {
            try
            {
                if (!skip.HasValue && !count.HasValue)
                    return StatusCode(200, new { mimes = await _mimeRepository.GetAll() });

                if (!skip.HasValue || !count.HasValue)
                    return StatusCode(400);

                return StatusCode(200, new { mimes = await _mimeRepository
                    .GetAll(query => query.Skip(skip.Value).Take(count.Value)) });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{mimeId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin,Admin")]
        public async Task<IActionResult> DeleteMime([FromRoute] int mimeId)
        {
            try
            {
                await _mimeRepository.Delete(mimeId);
                await _redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("many")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin,Admin")]
        public async Task<IActionResult> DeleteRangeMime([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await _mimeRepository.DeleteMany(identifiers);
                await _redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
