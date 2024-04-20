using application.Abstractions.Services.Inner;
using application.Abstractions.Services.TP_Services;
using application.DTO;
using application.DTO.Inner;
using application.Helpers;
using application.Helpers.Localization;
using application.Services.Abstractions;
using application.Services.Cache_Handlers;
using domain.Exceptions;
using domain.Models;
using System.Text.RegularExpressions;

namespace application.Services.Additional.Core
{
    public class CryptographyHelper(
        ICypher cypherFile,
        ICacheHandler<KeyStorageItemModel> cacheHandler,
        IValidation validation) : ICryptographyHelper
    {
        private const int TASK_AWAITING = 10000;

        private byte[] ConvertKey(string key)
        {
            if (!Regex.IsMatch(key, Validation.EncryptionKey) || !validation.IsBase64String(key))
                throw new FormatException(Message.INVALID_FORMAT);

            return Convert.FromBase64String(key);
        }

        public async Task<byte[]?> GetKey(int userId, int keyId, int storageId, string accessCode)
        {
            try
            {
                var cacheKey = $"{ImmutableData.STORAGE_ITEMS_PREFIX}{userId}_{keyId}_{storageId}";
                var key = await cacheHandler.CacheAndGet(
                    new StorageItemObject(cacheKey, userId, keyId, storageId, accessCode));

                if (key is null)
                    return null;

                return ConvertKey(key.key_value);
            }
            catch (EntityException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public async Task CypherFile(string filePath, string operation, byte[] key)
        {
            try
            {
                using var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var timeoutTask = Task.Delay(TASK_AWAITING);
                var cryptographyTask = cypherFile.CypherFileAsync(new CryptographyDTO
                {
                    FilePath = filePath,
                    Key = key,
                    Operation = operation,
                    CancellationToken = cancellationToken
                });

                var completedTask = await Task.WhenAny(cryptographyTask, timeoutTask);

                if (completedTask.Equals(timeoutTask))
                {
                    cts.Cancel();

                    if (File.Exists(filePath))
                        await Task.Run(() => File.Delete(filePath));

                    throw new InvalidOperationException(Message.TASK_TIMED_OUT);
                }
                else
                {
                    var cryptographyResult = await cryptographyTask;
                    if (!cryptographyResult.Success)
                        throw new InvalidOperationException(Message.BAD_CRYTOGRAPHY_DATA);
                }
            }
            catch (EntityException)
            {
                throw new InvalidOperationException(Message.ERROR);
            }
        }
    }
}
