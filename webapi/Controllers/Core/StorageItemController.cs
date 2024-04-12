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
using webapi.Services.Abstractions;

namespace webapi.Controllers.Core
{
    [Route("api/core/storage/item")]
    [ApiController]
    [Authorize]
    public class StorageItemController(
        [FromKeyedServices(ImplementationKey.CORE_KEY_STORAGE_SERVICE)] IValidator validator,
        IMapper mapper,
        IUserInfo userInfo,
        IPasswordManager passwordManager,
        IRepository<KeyStorageItemModel> storageItemRepository,
        IRepository<KeyStorageModel> storageRepository,
        IRedisCache redisCache) : ControllerBase
    {
        [HttpPost("{storageId}")]
        [EntityExceptionFilter]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddKey([FromRoute] int storageId, [FromQuery] int code, [FromBody] KeyDTO keyDTO)
        {
            if (!validator.IsValid(keyDTO.key_value))
                return StatusCode(422, new { message = Message.INVALID_FORMAT });

            if (!await IsValidStorage(storageId, userInfo.UserId, code))
                return StatusCode(400);

            var keyItemModel = mapper.Map<KeyDTO, KeyStorageItemModel>(keyDTO);
            keyItemModel.storage_id = storageId;
            keyItemModel.created_at = DateTime.UtcNow;

            await storageItemRepository.Add(keyItemModel);

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
            return StatusCode(201);
        }

        [HttpGet("{storageId}/{keyId}")]
        [EntityExceptionFilter]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetKey([FromRoute] int storageId, [FromRoute] int keyId, [FromQuery] int code)
        {
            if (!await IsValidStorage(storageId, userInfo.UserId, code))
                return StatusCode(400);

            var key = await storageItemRepository.GetById(keyId);
            if (key is null)
                return StatusCode(404, new { message = Message.NOT_FOUND });

            return StatusCode(200, new { key });
        }

        [HttpGet("all")]
        [EntityExceptionFilter]
        public async Task<IActionResult> GetAllKeys(int storageId, int code)
        {
            if (!await IsValidStorage(storageId, userInfo.UserId, code))
                return StatusCode(400);

            return StatusCode(200, new { keys = await storageItemRepository
                .GetAll(new StorageKeysByRelationSpec(storageId))});
        }

        [HttpDelete("{storageId}/{keyId}")]
        [EntityExceptionFilter]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteKey([FromRoute] int storageId, [FromRoute] int keyId, [FromQuery] int code)
        {
            if (!await IsValidStorage(storageId, userInfo.UserId, code))
                return StatusCode(400);

            await storageItemRepository.DeleteByFilter(new StorageKeyByIdAndRelationSpec(keyId, storageId));

            await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
            return StatusCode(204);
        }


        private async Task<bool> IsValidStorage(int storageId, int userId, int code)
        {
            try
            {
                var storage = await storageRepository
                    .GetByFilter(new StorageByIdAndRelationSpec(storageId, userId));

                if (storage is null || !passwordManager.CheckPassword(code.ToString(), storage.access_code))
                    return false;

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }
    }
}
