using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.DB.Abstractions;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/storage")]
    [ApiController]
    [Authorize]
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
            try
            {
                var keyStorageModel = mapper.Map<StorageDTO, KeyStorageModel>(storageDTO);
                keyStorageModel.user_id = userInfo.UserId;
                keyStorageModel.last_time_modified = DateTime.UtcNow;
                keyStorageModel.access_code = passwordManager.HashingPassword(storageDTO.access_code.ToString());
                await storageRepository.Add(keyStorageModel);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{storageId}")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> DeleteStorage([FromRoute] int storageId, [FromQuery] int code)
        {
            try
            {
                var storage = await storageRepository
                    .GetByFilter(query => query.Where(s => s.storage_id.Equals(storageId) && s.user_id.Equals(userInfo.UserId)));

                if (storage is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!passwordManager.CheckPassword(code.ToString(), storage.access_code))
                    return StatusCode(403, new { message = Message.INCORRECT });

                await storageRepository.Delete(storageId);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.STORAGES_PREFIX}{userInfo.UserId}");
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<KeyStorageModel>), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetStorages()
        {
            try
            {
                return StatusCode(200, new {
                    storages = await storageRepository.GetAll(query => query.Where(s => s.user_id.Equals(userInfo.UserId)))});
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{storageId}")]
        public async Task<IActionResult> GetStorage([FromRoute] int storageId, [FromQuery] int code)
        {
            try
            {
                var storage = await storageRepository
                    .GetByFilter(query => query.Where(s => s.storage_id.Equals(storageId) && s.user_id.Equals(userInfo.UserId)));
                if (storage is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!passwordManager.CheckPassword(code.ToString(), storage.access_code))
                    return StatusCode(403, new { message = Message.INCORRECT });
                storage.access_code = string.Empty;

                return StatusCode(200, new { storage });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
