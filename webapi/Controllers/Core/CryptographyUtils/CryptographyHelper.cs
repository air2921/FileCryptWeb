using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
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
        private readonly ICypher _cypherFile;
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
            ICypher cypherFile,
            IRepository<KeyModel> keyRepository,
            [FromKeyedServices("Decrypt")] ICypherKey decryptKey,
            ILogger<CryptographyHelper> logger,
            IConfiguration configuration,
            IValidation validation,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IUserInfo userInfo)
        {
            _fileService = fileService;
            _cypherFile = cypherFile;
            _keyRepository = keyRepository;
            _decryptKey = decryptKey;
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
        public async Task<IActionResult> EncryptFile(CryptographyOperationOptions options)
        {
            var filename = Guid.NewGuid().ToString() + "_" + options.File.FileName;
            var filePath = Path.Combine(DEFAULT_FOLDER, filename);

            try
            {
                var IsValidRoute = _fileService.CheckFileType(options.Type);
                if (!IsValidRoute)
                    return StatusCode(400, new { message = "Invalid route request" });

                var mimeCategory = _fileService.GetFileCategory(options.File.ContentType);

                if (!Directory.Exists(DEFAULT_FOLDER))
                    Directory.CreateDirectory(DEFAULT_FOLDER);

                var fileGood = await _fileService.CheckFile(options.File);
                if (!fileGood)
                    return StatusCode(415, new { message = Message.INFECTED_OR_INVALID });

                var sizeNotExceed = _fileService.CheckSize(options.File);
                if (!sizeNotExceed)
                    return StatusCode(422, new { message = Message.INVALID_FILE });

                var encryptionKey = CheckAndConvertKey(options.Key);

                await _fileService.UploadFile(filePath, options.File);

                await EncryptFile(filePath, options.Operation, encryptionKey, options.UserID, options.Username);

                await _fileService.CreateFile(options.UserID, filename, options.File.ContentType, mimeCategory, options.Type);

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                    FileShare.None, 4096, FileOptions.DeleteOnClose);

                return File(fileStream, options.File.ContentType, filename);
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

        private async Task EncryptFile(string filePath, string operation, byte[] key, int? id, string? username)
        {
            try
            {
                using var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var timeoutTask = Task.Delay(TASK_AWAITING);
                var cryptographyTask = _cypherFile.CypherFileAsync(new CryptographyData
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
            catch (CryptographicException ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(EncryptFile));
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

    public class CryptographyOperationOptions
    {
        public string Key { get; init; }
        public IFormFile File { get; init; }
        public int UserID { get; init; }
        public string? Username { get; init; }
        public string Type { get; init; }
        public string Operation { get; init; }
    }
}
