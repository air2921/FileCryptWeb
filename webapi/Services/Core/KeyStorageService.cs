using System.Security.Cryptography;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Core
{
    public interface IStorageHelpers
    {
        Task<KeyStorageModel> GetAndValidateStorage(int storageId, int userId, int code);
        Task<IEnumerable<KeyStorageItemModel>> CypherKeys(IEnumerable<KeyStorageItemModel> keys, bool encrypt);
    }

    public class KeyStorageService(
        IRepository<KeyStorageModel> storageRepository,
        IConfiguration configuration,
        [FromKeyedServices("Decrypt")] ICypherKey decryptKey,
        [FromKeyedServices("Encrypt")] ICypherKey encryptKey,
        IPasswordManager passwordManager,
        IValidation validation) : IValidator, IStorageHelpers
    {
        private readonly byte[] secretKey = Convert.FromBase64String(configuration[App.ENCRYPTION_KEY]!);

        [Helper]
        public async Task<KeyStorageModel> GetAndValidateStorage(int storageId, int userId, int code)
        {
            try
            {
                var storage = await storageRepository.GetByFilter(query => query
                    .Where(s => s.user_id.Equals(userId) && s.storage_id.Equals(storageId))) ??
                        throw new ArgumentNullException(Message.NOT_FOUND);

                if (!passwordManager.CheckPassword(code.ToString(), storage.access_code))
                    throw new ArgumentException(Message.INCORRECT);

                return storage;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        [Helper]
        public async Task<IEnumerable<KeyStorageItemModel>> CypherKeys(IEnumerable<KeyStorageItemModel> keys, bool encrypt)
        {
            foreach (var key in keys)
            {
                try
                {
                    if (encrypt)
                        key.key_value = await encryptKey.CypherKeyAsync(key.key_value, secretKey);
                    else
                        key.key_value = await decryptKey.CypherKeyAsync(key.key_value, secretKey);
                }
                catch (CryptographicException)
                {
                    continue;
                }
            }
            return keys;
        }

        public bool IsValid(object key, object parameter = null)
        {
            if (key is not string)
                return false;

            return !string.IsNullOrWhiteSpace((string)key) && validation.IsBase64String((string)key) && Regex.IsMatch((string)key, Validation.EncryptionKey);
        }
    }
}
