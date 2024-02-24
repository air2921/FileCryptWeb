using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using webapi.Attributes;
using webapi.DTO;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Core
{
    [Route("api/core/keys/storage")]
    [ApiController]
    [Authorize]
    public class KeyStorageController : ControllerBase
    {
        #region fields and constructor

        private readonly IRepository<KeyStorageModel> _storageRepository;
        private readonly IRepository<KeyStorageItemModel> _storageItemRepository;
        private readonly ICypherKey _decryptKey;
        private readonly ICypherKey _encryptKey;
        private readonly IPasswordManager _passwordManager;
        private readonly IValidation _validation;
        private readonly IUserInfo _userInfo;
        private readonly IConfiguration _configuration;
        private readonly byte[] secretKey;

        public KeyStorageController(
            IRepository<KeyStorageModel> storageRepository,
            IRepository<KeyStorageItemModel> storageItemRepository,
            IEnumerable<ICypherKey> cypherKeys,
            IImplementationFinder implementationFinder,
            IPasswordManager passwordManager,
            IValidation validation,
            IUserInfo userInfo,
            IConfiguration configuration)
        {
            _storageRepository = storageRepository;
            _storageItemRepository = storageItemRepository;
            _decryptKey = implementationFinder.GetImplementationByKey(cypherKeys, ImplementationKey.DECRYPT_KEY);
            _encryptKey = implementationFinder.GetImplementationByKey(cypherKeys, ImplementationKey.ENCRYPT_KEY);
            _passwordManager = passwordManager;
            _validation = validation;
            _userInfo = userInfo;
            _configuration = configuration;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        #endregion

        [HttpPost]
        [XSRFProtection]
        public async Task<IActionResult> AddStorage([FromBody] StorageDTO storageDTO)
        {
            try
            {
                await _storageRepository.Add(new KeyStorageModel
                {
                    user_id = _userInfo.UserId,
                    storage_name = storageDTO.storage_name,
                    last_time_modified = DateTime.UtcNow,
                    access_code = _passwordManager.HashingPassword(storageDTO.storage_code.ToString()),
                    encrypt = storageDTO.encrypt
                });

                return StatusCode(201);
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{storageId}")]
        [XSRFProtection]
        public async Task<IActionResult> UpdateStorage([FromRoute] int storageId, [FromQuery] int code,
            [FromBody] UpdateStorageDTO storageDTO)
        {
            try
            {
                var storage = await _storageRepository.GetByFilter(query => query
                    .Where(s => s.user_id.Equals(_userInfo.UserId) && s.storage_id.Equals(storageId)));
                if (storage is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!CheckCode(code, storage.access_code))
                    return StatusCode(403, new { message = Message.INCORRECT });

                await DbUpdate(storageDTO, storage);

                return StatusCode(200, new { message = Message.UPDATED });
            }
            catch (EntityNotUpdatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{storageId}")]
        public async Task<IActionResult> GetStorageAndItems([FromRoute] int storageId, [FromQuery] int code)
        {
            try
            {
                var storage = await _storageRepository.GetByFilter(query => query
                    .Where(s => s.user_id.Equals(_userInfo.UserId) && s.storage_id.Equals(storageId)));
                if (storage is null)
                    return StatusCode(404, new { message = Message.NOT_FOUND });

                if (!CheckCode(code, storage.access_code))
                    return StatusCode(403, new { message = Message.INCORRECT });

                storage.access_code = string.Empty;

                var keys = await _storageItemRepository.GetAll(query => query.Where(s => s.storage_id.Equals(storage.storage_id)));
                if (storage.encrypt)
                    keys = await CypherKeys(keys, false);

                return StatusCode(200, new { storage, keys });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetStorages()
        {
            return StatusCode(200, new { storages = await _storageRepository
                .GetAll(query => query.Where(s => s.user_id.Equals(_userInfo.UserId))) });
        }

        [HttpDelete("{storageId}")]
        [XSRFProtection]
        public async Task<IActionResult> DeleteStorage([FromRoute] int storageId)
        {
            try
            {
                await _storageRepository.Delete(storageId);
                return StatusCode(204);
            }
            catch (EntityNotDeletedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Helper]
        private bool CheckCode(int input, string correct)
        {
            return _passwordManager.CheckPassword(input.ToString(), correct);
        }

        [Helper]
        private async Task<IEnumerable<KeyStorageItemModel>> CypherKeys(IEnumerable<KeyStorageItemModel> keys, bool encrypt)
        {

            foreach (var key in keys)
            {
                try
                {
                    if (encrypt)
                        key.key_value = await _encryptKey.CypherKeyAsync(key.key_value, secretKey);
                    else
                        key.key_value = await _decryptKey.CypherKeyAsync(key.key_value, secretKey);
                }
                catch (CryptographicException)
                {
                    continue;
                }
            }

            return keys;
        }

        [Helper]
        private async Task DbUpdate(UpdateStorageDTO storageDTO, KeyStorageModel keyStorageModel)
        {
            var keys = new List<KeyStorageItemModel>();

            try
            {
                if (storageDTO.encrypt is not null)
                {
                    bool shouldEncrypt = storageDTO.encrypt.Value;
                    if (keyStorageModel.encrypt != shouldEncrypt)
                    {
                        keys = (List<KeyStorageItemModel>)await CypherKeys(await _storageItemRepository
                            .GetAll(query => query.Where(s => s.storage_id.Equals(keyStorageModel.storage_id))), shouldEncrypt);
                    }
                }

                if (!string.IsNullOrWhiteSpace(storageDTO.storage_name) && !storageDTO.storage_name.Equals(keyStorageModel.storage_name))
                    keyStorageModel.storage_name = storageDTO.storage_name;

                if (storageDTO.encrypt.HasValue && !storageDTO.encrypt.Equals(keyStorageModel.encrypt))
                    keyStorageModel.encrypt = storageDTO.encrypt.Value;

                if (storageDTO.access_code.HasValue && _validation.IsSixDigit(storageDTO.access_code.Value))
                    keyStorageModel.access_code = _passwordManager.HashingPassword(storageDTO.access_code.Value.ToString());

                keyStorageModel.last_time_modified = DateTime.UtcNow;

                await _storageRepository.Update(keyStorageModel);
            }
            catch (EntityNotUpdatedException)
            {
                throw;
            }
        }
    }
}
