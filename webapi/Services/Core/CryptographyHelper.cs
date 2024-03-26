using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using webapi.Cryptography;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Core
{
    public class CryptographyHelper(
        ICypher cypherFile,
        IRepository<KeyModel> keyRepository,
        [FromKeyedServices("Decrypt")] ICypherKey decryptKey,
        ILogger<CryptographyHelper> logger,
        IConfiguration configuration,
        IValidation validation,
        IRedisCache redisCache,
        IRedisKeys redisKeys) : ICryptographyHelper
    {

        private const int TASK_AWAITING = 10000;
        private readonly string privateType = FileType.Private.ToString().ToLowerInvariant();
        private readonly string internalType = FileType.Internal.ToString().ToLowerInvariant();
        private readonly string receivedType = FileType.Received.ToString().ToLowerInvariant();
        private readonly byte[] secretKey = Convert.FromBase64String(configuration[App.ENCRYPTION_KEY]!);

        public byte[] CheckAndConvertKey(string key)
        {
            if (!Regex.IsMatch(key, Validation.EncryptionKey) || !validation.IsBase64String(key))
                throw new FormatException(Message.INVALID_FORMAT);

            return Convert.FromBase64String(key);
        }

        public async Task EncryptFile(string filePath, string operation, byte[] key, int? id, string? username)
        {
            try
            {
                using var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var timeoutTask = Task.Delay(TASK_AWAITING);
                var cryptographyTask = cypherFile.CypherFileAsync(new CryptographyData
                {
                    FilePath = filePath,
                    Key = key,
                    Operation = operation,
                    CancellationToken = cancellationToken,
                    UserId = id,
                    Username = username
                });

                var completedTask = await Task.WhenAny(cryptographyTask, timeoutTask);

                if (completedTask == timeoutTask)
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
            catch (CryptographicException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(EncryptFile));
                throw new InvalidOperationException(Message.BAD_CRYTOGRAPHY_DATA);
            }
        }

        public async Task<string> CacheKey(string key, int userId)
        {
            try
            {
                var value = await redisCache.GetCachedData(key);

                if (value is not null)
                    return JsonConvert.DeserializeObject<string>(value);

                var keys = await keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userId)));
                if (keys is null)
                    throw new ArgumentNullException(Message.NOT_FOUND);

                string? encryptionKey = null;

                if (key == redisKeys.PrivateKey)
                    encryptionKey = keys.private_key;
                else if (key == redisKeys.InternalKey)
                    encryptionKey = keys.internal_key;
                else if (key == redisKeys.ReceivedKey)
                    encryptionKey = keys.received_key;
                else
                    throw new ArgumentException(Message.NOT_FOUND);

                if (string.IsNullOrEmpty(encryptionKey))
                    throw new ArgumentNullException(Message.NOT_FOUND);

                var decryptedKey = await decryptKey.CypherKeyAsync(encryptionKey, secretKey);
                await redisCache.CacheData(key, decryptedKey, TimeSpan.FromMinutes(10));

                return decryptedKey;
            }
            catch (OperationCanceledException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
        }
    }
}
