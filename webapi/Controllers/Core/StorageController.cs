using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef.Specifications.By_Relation_Specifications;
using webapi.DTO;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/storage")]
    [ApiController]
    [Authorize]
    [EntityExceptionFilter]
    public class StorageController(
        IMapper mapper,
        IUserInfo userInfo,
        IPasswordManager passwordManager,
        IRepository<KeyStorageModel> storageRepository,
        IRedisCache redisCache) : ControllerBase
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddStorage([FromBody] StorageDTO storageDTO)
        {
            var keyStorageModel = mapper.Map<StorageDTO, KeyStorageModel>(storageDTO);
            keyStorageModel.user_id = userInfo.UserId;
            keyStorageModel.last_time_modified = DateTime.UtcNow;
            keyStorageModel.access_code = passwordManager.HashingPassword(storageDTO.access_code.ToString());
            await storageRepository.Add(keyStorageModel);

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
            return StatusCode(201);
        }

        [HttpDelete("{storageId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteStorage([FromRoute] int storageId, [FromQuery] int code)
        {
            var storage = await storageRepository
                .GetByFilter(new StorageByIdAndRelationSpec(storageId, userInfo.UserId));

            if (storage is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (!passwordManager.CheckPassword(code.ToString(), storage.access_code))
                return StatusCode(403, new { message = Message.INCORRECT });

            await storageRepository.Delete(storageId);

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
            return StatusCode(204);
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<KeyStorageModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetStorages()
        {
            return StatusCode(200, new {
                storages = await storageRepository.GetAll(new StoragesByRelationSpec(userInfo.UserId))});
        }

        [HttpGet("{storageId}")]
        public async Task<IActionResult> GetStorage([FromRoute] int storageId, [FromQuery] int code)
        {
            var storage = await storageRepository
                .GetByFilter(new StorageByIdAndRelationSpec(storageId, userInfo.UserId));
            if (storage is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            if (!passwordManager.CheckPassword(code.ToString(), storage.access_code))
                return StatusCode(403, new { message = Message.INCORRECT });
            storage.access_code = string.Empty;

            return StatusCode(200, new { storage });
        }
    }
}
