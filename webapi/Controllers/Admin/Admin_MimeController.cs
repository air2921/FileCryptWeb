using Microsoft.AspNetCore.Mvc;
using webapi.Exceptions;
using webapi.Localization;
using webapi.Models;
using Microsoft.AspNetCore.Authorization;
using webapi.Helpers;
using webapi.DB.Abstractions;
using webapi.Helpers.Abstractions;
using webapi.DB.Ef.Specifications.Sorting_Specifications;
using webapi.Attributes;

namespace webapi.Controllers.Admin
{
    [Route("api/admin/mime")]
    [ApiController]
    [EntityExceptionFilter]
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
            await mimeRepository.Add(new FileMimeModel { mime_name = mime });
            await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);

            return StatusCode(201, new { message = Message.CREATED });
        }

        [HttpPost("range")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateMIMICollection()
        {
            var mimeModels = new HashSet<FileMimeModel>();
            var mimes = (await mimeRepository.GetAll()).Select(m => m.mime_name).ToHashSet();

            fileManager.AddMimeCollection(ref mimeModels, mimes);
            await mimeRepository.AddRange(mimeModels);
            await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);

            return StatusCode(201, new { message = Message.CREATED });
        }

        [HttpGet("{mimeId}")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(typeof(FileMimeModel), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetMime([FromRoute] int mimeId)
        {
            var mime = await mimeRepository.GetById(mimeId);
            if (mime is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return StatusCode(200, new { mime });
        }

        [HttpGet("range")]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(typeof(IEnumerable<FileMimeModel>), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetMimes([FromQuery] int skip, [FromQuery] int count)
        {
            return StatusCode(200, new { mimes = await mimeRepository
                .GetAll(new MimesSortSpec(skip, count))});
        }

        [HttpDelete("{mimeId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteMime([FromRoute] int mimeId)
        {
            var mime = await mimeRepository.Delete(mimeId);
            if (mime is not null)
                await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);

            return StatusCode(204);
        }

        [HttpDelete("range")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HighestAdmin,Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteMimes([FromBody] IEnumerable<int> identifiers)
        {
            await mimeRepository.DeleteMany(identifiers);
            await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
            return StatusCode(204);
        }
    }
}
