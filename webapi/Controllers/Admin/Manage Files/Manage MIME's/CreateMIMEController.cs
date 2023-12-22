using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Interfaces.Redis;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers.Admin.Manage_Files.Manage_MIME_s
{
    [Route("api/admin/mime")]
    [ApiController]
    [Authorize(Roles = "HighestAdmin")]
    [ValidateAntiForgeryToken]
    public class CreateMIMEController : ControllerBase
    {
        private readonly ILogger<CreateMIMEController> _logger;
        private readonly IRedisCache _redisCache;
        private readonly ICreate<FileMimeModel> _createMime;
        private readonly IInsertBase<FileMimeModel> _insertBase;

        public CreateMIMEController(ILogger<CreateMIMEController> logger, IRedisCache redisCache, ICreate<FileMimeModel> createMime, IInsertBase<FileMimeModel> insertBase)
        {
            _createMime = createMime;
            _redisCache = redisCache;
            _insertBase = insertBase;
            _logger = logger;
        }

        [HttpPost("add/new")]
        public async Task<IActionResult> CreateNewMime([FromBody] FileMimeModel mimeModel)
        {
            await _createMime.Create(mimeModel);
            await _redisCache.DeleteCache(Constants.MIME_COLLECTION);
            _logger.LogWarning($"new MIME type: {mimeModel.mime_name}. Added in db");

            return StatusCode(201, new { message = SuccessMessage.SuccessCreatedNewMime });
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateSecureBaseMIMICollection([FromQuery] bool secure)
        {
            if (secure)
            {
                _logger.LogWarning("Safe MIME collection was added in db");
            }
            else
            {
                _logger.LogWarning("Unsafe MIME collection was added in db");
            }

            await _insertBase.DBInsertBase(null, secure);
            await _redisCache.DeleteCache(Constants.MIME_COLLECTION);

            return StatusCode(201, new { message = SuccessMessage.SuccessMimeCollectionCreate });
        }
    }
}
