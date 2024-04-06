﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.Abstractions;
using webapi.DTO;
using webapi.Exceptions;
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
        [ValidateAntiForgeryToken]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> AddKey([FromRoute] int storageId, [FromQuery] int code, [FromBody] KeyDTO keyDTO)
        {
            try
            {
                if (!validator.IsValid(keyDTO.key_value))
                    return StatusCode(422, new { message = Message.INVALID_FORMAT });

                if (await IsValidStorage(storageId, userInfo.UserId, code))
                    return StatusCode(400);

                var keyItemModel = mapper.Map<KeyDTO, KeyStorageItemModel>(keyDTO);
                keyItemModel.storage_id = storageId;
                keyItemModel.created_at = DateTime.UtcNow;

                await storageItemRepository.Add(keyItemModel);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{storageId}/{keyId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetKey([FromRoute] int storageId, [FromRoute] int keyId, [FromQuery] int code)
        {
            try
            {
                if (await IsValidStorage(storageId, userInfo.UserId, code))
                    return StatusCode(400);

                var key = await storageItemRepository.GetById(keyId);
                if (key is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                return StatusCode(200, new { key });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllKeys(int storageId, int code)
        {
            try
            {
                if (await IsValidStorage(storageId, userInfo.UserId, code))
                    return StatusCode(400);

                return StatusCode(200, new {
                    keys = await storageItemRepository
                    .GetAll(query => query.Where(s => s.storage_id.Equals(storageId)))});
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{storageId}/{keyId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteKey([FromRoute] int storageId, [FromRoute] int keyId, [FromQuery] int code)
        {
            try
            {
                if (await IsValidStorage(storageId, userInfo.UserId, code))
                    return StatusCode(400);

                await storageItemRepository.DeleteByFilter(query => query
                    .Where(s => s.key_id.Equals(keyId) && s.storage_id.Equals(storageId)));

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(204);
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private async Task<bool> IsValidStorage(int storageId, int userId, int code)
        {
            var storage = await storageRepository
                .GetByFilter(query => query.Where(s => s.storage_id.Equals(storageId) && s.user_id.Equals(userId)));

            if (storage is null)
                return false;

            if (!passwordManager.CheckPassword(code.ToString(), storage.access_code))
                return false;

            return true;
        }
    }
}
