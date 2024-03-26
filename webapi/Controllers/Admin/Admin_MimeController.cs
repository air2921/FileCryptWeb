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
    public class Admin_MimeController(
        IRepository<FileMimeModel> mimeRepository,
        ILogger<Admin_MimeController> logger,
        IRedisCache redisCache,
        IFileManager fileManager) : ControllerBase
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateNewMime([FromQuery] string mime)
        {
            try
            {
                await mimeRepository.Add(new FileMimeModel { mime_name = mime });
                await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                logger.LogWarning($"new MIME type: {mime}. Added in db");

                return StatusCode(201, new { message = Message.CREATED });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("range")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateMIMICollection()
        {
            try
            {
                var mimeModels = new HashSet<FileMimeModel>();
                var existingMimes = await mimeRepository.GetAll();
                var mimes = existingMimes.Select(m => m.mime_name).ToHashSet();

                fileManager.AddMimeCollection(ref mimeModels, mimes);
                await mimeRepository.AddRange(mimeModels);
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
                await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
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
                var mime = await mimeRepository.GetById(mimeId);
                if (mime is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { mime });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("range")]
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

                if (skip.HasValue && count.HasValue)
                    return StatusCode(200, new { mimes = await mimeRepository
                        .GetAll(query => query.Skip(skip.Value).Take(count.Value))});
                else
                    return StatusCode(200, new { mimes = await mimeRepository.GetAll() });
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
                await mimeRepository.Delete(mimeId);
                await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteMimes([FromBody] IEnumerable<int> identifiers)
        {
            try
            {
                await mimeRepository.DeleteMany(identifiers);
                await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
