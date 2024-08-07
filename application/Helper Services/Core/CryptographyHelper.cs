﻿using application.Abstractions.Inner;
using application.Abstractions.TP_Services;
using application.Cache_Handlers;
using application.DTO.Inner;
using application.Helpers;
using application.Helpers.Localization;
using domain.Exceptions;
using domain.Models;
using System.Text.RegularExpressions;

namespace application.Helper_Services.Core
{
    public class CryptographyHelper(
        ICypher cypherFile,
        ICacheHandler<KeyStorageItemModel> cacheHandler) : ICryptographyHelper
    {
        private const int TASK_AWAITING = 10000;

        private byte[] ConvertKey(string key)
        {
            if (!Regex.IsMatch(key, RegularEx.EncryptionKey) || !IsBase64String(key))
                throw new FormatException(Message.INVALID_FORMAT);

            return Convert.FromBase64String(key);
        }

        private bool IsBase64String(string? key)
        {
            if (string.IsNullOrEmpty(key) || key.Length % 4 != 0)
                return false;

            try
            {
                return Convert.FromBase64String(key).Length.Equals(32);
            }
            catch (FormatException)
            {
                return false;
            }
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
