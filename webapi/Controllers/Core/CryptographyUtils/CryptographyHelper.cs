using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using webapi.Attributes;
using webapi.Cryptography;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Cryptography;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Controllers.Base
{
    public class CryptographyHelper : ControllerBase, ICryptographyControllerBase, ICryptographyParamsProvider
    {
        private const string DEFAULT_FOLDER = "C:\\FileCryptWeb";
        private const int TASK_AWAITING = 10000;

        private readonly string privateType = FileType.Private.ToString().ToLowerInvariant();
        private readonly string internalType = FileType.Internal.ToString().ToLowerInvariant();
        private readonly string receivedType = FileType.Received.ToString().ToLowerInvariant();

        #region fields and constructor

        private readonly IFileService _fileService;
        private readonly IRepository<KeyModel> _keyRepository;
        private readonly ICypherKey _decryptKey;
        private readonly ILogger<CryptographyHelper> _logger;
        private readonly IConfiguration _configuration;
        private readonly IValidation _validation;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;
        private readonly byte[] secretKey;

        public CryptographyHelper(
            IFileService fileService,
            IRepository<KeyModel> keyRepository,
            IEnumerable<ICypherKey> cypherKeys,
            IImplementationFinder implementationFinder,
            ILogger<CryptographyHelper> logger,
            IConfiguration configuration,
            IValidation validation,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IUserInfo userInfo)
        {
            _fileService = fileService;
            _keyRepository = keyRepository;
            _decryptKey = implementationFinder.GetImplementationByKey(cypherKeys, ImplementationKey.DECRYPT_KEY);
            _logger = logger;
            _configuration = configuration;
            _validation = validation;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
            secretKey = Convert.FromBase64String(_configuration[App.ENCRYPTION_KEY]!);
        }

        #endregion

        [NonAction]
        [Helper]
        public async Task<IActionResult> EncryptFile(
            Func<string, byte[], CancellationToken, string, Task<CryptographyResult>> CryptographyFunction,
            string key, IFormFile file,
            int userID, string type, string operation)
        {

            var filename = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(DEFAULT_FOLDER, filename);

            try
            {
                var IsValidRoute = _fileService.CheckFileType(type);
                if (!IsValidRoute)
                    return StatusCode(400, new { message = "Invalid route request" });

                var mimeCategory = _fileService.GetFileCategory(file.ContentType);

                if (!Directory.Exists(DEFAULT_FOLDER))
                    Directory.CreateDirectory(DEFAULT_FOLDER);

                var fileGood = await _fileService.CheckFile(file);
                if (!fileGood)
                    return StatusCode(415, new { message = Message.INFECTED_OR_INVALID });

                var sizeNotExceed = _fileService.CheckSize(file);
                if (!sizeNotExceed)
                    return StatusCode(422, new { message = Message.INVALID_FILE });

                var encryptionKey = CheckAndConvertKey(key);

                await _fileService.UploadFile(filePath, file);

                await EncryptFile(filePath, operation, CryptographyFunction, encryptionKey);

                await _fileService.CreateFile(userID, filename, file.ContentType, mimeCategory, type);

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                    FileShare.None, 4096, FileOptions.DeleteOnClose);

                return File(fileStream, file.ContentType, filename);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (FormatException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
            catch (EntityNotCreatedException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    System.IO.File.Delete(filePath);
                    System.IO.File.Delete($"{filePath}.tmp");

                    return StatusCode(422, new { message = ex.Message });
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc.ToString());
                    return StatusCode(422, new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, new { message = Message.ERROR });
            }
        }

        private byte[] CheckAndConvertKey(string key)
        {
            if (!Regex.IsMatch(key, Validation.EncryptionKey) || !_validation.IsBase64String(key))
                throw new FormatException(Message.INVALID_FORMAT);

            return Convert.FromBase64String(key);
        }

        private async Task EncryptFile(string filePath, string operation, Func<string, byte[], CancellationToken, string, Task<CryptographyResult>> CryptographyFunction, byte[] key)
        {
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var cryptographyTask = CryptographyFunction(filePath, key, cancellationToken, operation);
            var timeoutTask = Task.Delay(TASK_AWAITING);

            var completedTask = await Task.WhenAny(cryptographyTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                cts.Cancel();

                if (System.IO.File.Exists(filePath))
                    await Task.Run(() => System.IO.File.Delete(filePath));

                throw new InvalidOperationException(Message.TASK_TIMED_OUT);
            }
            else
            {
                var cryptographyResult = await cryptographyTask;
                if (!cryptographyResult.Success)
                    throw new InvalidOperationException(Message.BAD_CRYTOGRAPHY_DATA);
            }
        }

        public async Task<CryptographyParams> GetCryptographyParams(string fileType, string operation)
        {
            string lowerFileType = fileType.ToLowerInvariant();
            bool isValidRoute = false;

            if (operation == "encrypt")
                isValidRoute = true;
            else if (operation == "decrypt")
                isValidRoute = true;
            else
                throw new InvalidRouteException();

            try
            {
                if (lowerFileType == privateType)
                    return new CryptographyParams(await CacheKey(_redisKeys.PrivateKey, _userInfo.UserId), isValidRoute);
                else if (lowerFileType == internalType)
                    return new CryptographyParams(await CacheKey(_redisKeys.InternalKey, _userInfo.UserId), isValidRoute);
                else if (lowerFileType == receivedType)
                    return new CryptographyParams(await CacheKey(_redisKeys.ReceivedKey, _userInfo.UserId), isValidRoute);
                else
                    throw new InvalidRouteException();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw new InvalidRouteException();
            }
        }

        private async Task<string> CacheKey(string key, int userId)
        {
            try
            {
                var value = await _redisCache.GetCachedData(key);

                if (value is not null)
                    return JsonConvert.DeserializeObject<string>(value);

                var keys = await _keyRepository.GetByFilter(query => query.Where(k => k.user_id.Equals(userId)));
                if (keys is null)
                    throw new ArgumentNullException(Message.NOT_FOUND);

                string? encryptionKey = null;

                if (key == _redisKeys.PrivateKey)
                    encryptionKey = keys.private_key;
                else if (key == _redisKeys.InternalKey)
                    encryptionKey = keys.internal_key;
                else if (key == _redisKeys.ReceivedKey)
                    encryptionKey = keys.received_key;
                else
                    throw new ArgumentException();

                if (string.IsNullOrEmpty(encryptionKey))
                    throw new ArgumentNullException(Message.NOT_FOUND);

                var decryptedKey = await _decryptKey.CypherKeyAsync(encryptionKey, secretKey);
                await _redisCache.CacheData(key, decryptedKey, TimeSpan.FromMinutes(10));

                return decryptedKey;
            }
            catch (OperationCanceledException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
        }
    }

    public record CryptographyParams(string EncryptionKey, bool IsValidRoute);
}
