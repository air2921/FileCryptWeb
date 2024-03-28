using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services.Core;

namespace webapi.Controllers.Core
{
    [Route("api/core/keys")]
    [ApiController]
    [Authorize]
    public class KeysController(
        IRepository<KeyModel> keyRepository,
        IConfiguration configuration,
        IGenerate generate,
        IRedisCache redisCache,
        IRedisKeys redisKeys,
        IUserInfo userInfo,
        [FromKeyedServices("Decrypt")] ICypherKey decrypt,
        [FromKeyedServices(ImplementationKey.CORE_KEY_SERVICE)] IValidator validator,
        [FromKeyedServices(ImplementationKey.CORE_KEY_SERVICE)] IDataManagement dataManagement,
        IKeyHelper helper) : ControllerBase
    {
        private readonly byte[] secretKey = Convert.FromBase64String(configuration[App.ENCRYPTION_KEY]!);

        [HttpGet("all")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> GetAllKeys()
        {
            try
            {
                var keys = new KeyModel();
                var cacheKey = $"{ImmutableData.KEYS_PREFIX}{userInfo.UserId}";
                var cache = await redisCache.GetCachedData(cacheKey);
                if (cache is null)
                {
                    keys = await keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userInfo.UserId)));
                    if (keys is null)
                        return StatusCode(404, new { message = Message.NOT_FOUND });

                    await redisCache.CacheData(cacheKey, keys, TimeSpan.FromMinutes(10));
                }
                else
                    keys = JsonConvert.DeserializeObject<KeyModel>(cache);

                keys!.private_key = await decrypt.CypherKeyAsync(keys.private_key, secretKey);
                keys.internal_key = keys.internal_key is not null ? await decrypt.CypherKeyAsync(keys.internal_key, secretKey) : null;
                keys.received_key = keys.received_key is not null ? "hidden" : null;

                return StatusCode(200, new { keys = new { keys.private_key, keys.internal_key, keys.received_key } });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("private")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UpdatePrivateKey([FromQuery] string? key)
        {
            try
            {
                key = validator.IsValid(key) ? key : generate.GenerateKey();
                await helper.UpdateKey(key, userInfo.UserId, FileType.Private);

                await dataManagement.DeleteData(userInfo.UserId, redisKeys.PrivateKey);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (CryptographicException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpPut("internal")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UpdatePersonalInternalKey([FromQuery] string? key)
        {
            try
            {
                key = validator.IsValid(key) ? key : generate.GenerateKey();
                await helper.UpdateKey(key, userInfo.UserId, FileType.Private);

                await dataManagement.DeleteData(userInfo.UserId, redisKeys.InternalKey);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (CryptographicException)
            {
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        [HttpPut("received/clean")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> CleanReceivedInternalKey()
        {
            try
            {
                var keys = await keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userInfo.UserId)));
                if (keys.received_key is null)
                    return StatusCode(409, new { message = Message.CONFLICT });

                keys.received_key = null;
                await keyRepository.Update(keys);

                await dataManagement.DeleteData(userInfo.UserId, redisKeys.ReceivedKey);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
