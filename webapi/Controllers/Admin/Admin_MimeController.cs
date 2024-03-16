using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces;
using webapi.Localization;
using webapi.Models;
using Microsoft.AspNetCore.Authorization;
using webapi.Helpers;
using webapi.Attributes;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/mime")]
    [ApiController]
    public class Admin_MimeController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<FileMimeModel> _mimeRepository;
        private readonly ILogger<Admin_MimeController> _logger;
        private readonly IRedisCache _redisCache;
        private readonly IFileManager _fileManager;

        public Admin_MimeController(
            IRepository<FileMimeModel> mimeRepository,
            ILogger<Admin_MimeController> logger,
            IRedisCache redisCache,
            IFileManager fileManager)
        {
            _mimeRepository = mimeRepository;
            _redisCache = redisCache;
            _logger = logger;
            _fileManager = fileManager;
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateNewMime([FromQuery] string mime)
        {
            try
            {
                await _mimeRepository.Add(new FileMimeModel { mime_name = mime });
                await _redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                _logger.LogWarning($"new MIME type: {mime}. Added in db");

                return StatusCode(201, new { message = Message.CREATED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("many")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateMIMICollection([FromQuery] bool secured)
        {
            try
            {
                var mimeModels = new HashSet<FileMimeModel>();
                var existingMimes = await _mimeRepository.GetAll();
                var mimes = existingMimes.Select(m => m.mime_name).ToHashSet();

                if (!secured)
                    _fileManager.AddFullCollection(ref mimeModels, mimes);
                else
                    _fileManager.AddSecureCollection(ref mimeModels, mimes);

                await _mimeRepository.AddRange(mimeModels);
                return StatusCode(201, new { message = Message.CREATED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            finally
            {
                await _redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
            }
        }

        [HttpGet("{mimeId}")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(typeof(FileMimeModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetMime([FromRoute] int mimeId)
        {
            try
            {
                var mime = await _mimeRepository.GetById(mimeId);
                if (mime is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { mime });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(typeof(IEnumerable<FileMimeModel>), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetMimes([FromQuery] int? skip, [FromQuery] int? count)
        {
            try
            {
                if ((!skip.HasValue && count.HasValue) || (skip.HasValue && !count.HasValue))
                    return StatusCode(400);

                if (!skip.HasValue && !count.HasValue)
                    return StatusCode(200, new { mimes = await _mimeRepository.GetAll() });

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
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
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
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteMimes([FromBody] IEnumerable<int> identifiers)
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
