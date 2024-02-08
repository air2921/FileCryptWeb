﻿using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using webapi.Exceptions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;
using webapi.Services;
using webapi.Services.Cryptography;

namespace webapi.Controllers.Base
{
    public class CryptographyHelper : ControllerBase, ICryptographyControllerBase, ICryptographyParamsProvider
    {
        private const string DEFAULT_FOLDER = "C:\\FileCryptWeb";
        private const int TASK_AWAITING = 30000;

        private readonly string privateType = FileType.Private.ToString().ToLowerInvariant();
        private readonly string internalType = FileType.Internal.ToString().ToLowerInvariant();
        private readonly string receivedType = FileType.Received.ToString().ToLowerInvariant();

        private readonly IFileService _fileService;
        private readonly ILogger<CryptographyHelper> _logger;
        private readonly IValidation _validation;
        private readonly IRedisCache _redisCache;
        private readonly IRedisKeys _redisKeys;
        private readonly IUserInfo _userInfo;

        public CryptographyHelper(
            IFileService fileService,
            ILogger<CryptographyHelper> logger,
            IValidation validation,
            IRedisCache redisCache,
            IRedisKeys redisKeys,
            IUserInfo userInfo)
        {
            _fileService = fileService;
            _logger = logger;
            _validation = validation;
            _redisCache = redisCache;
            _redisKeys = redisKeys;
            _userInfo = userInfo;
        }

        public async Task<IActionResult> EncryptFile(
            Func<string, byte[], CancellationToken, Task<CryptographyResult>> CryptographyFunction,
            string key, IFormFile file,
            int userID, string type)
        {

            var filename = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(DEFAULT_FOLDER, filename);

            try
            {
                var IsValidRoute = _fileService.CheckFileType(type);
                if (!IsValidRoute)
                    return StatusCode(400, new { message = "Invalid route request" });

                if (!Directory.Exists(DEFAULT_FOLDER))
                    Directory.CreateDirectory(DEFAULT_FOLDER);

                var fileGood = await _fileService.CheckFile(file);
                if (!fileGood)
                    return StatusCode(415, new { message = ErrorMessage.InfectedOrInvalid });

                var sizeNotExceed = _fileService.CheckSize(file);
                if (!sizeNotExceed)
                    return StatusCode(422, new { message = ErrorMessage.ExceedMaxSize });

                var encryptionKey = CheckAndConvertKey(key);

                await _fileService.UploadFile(filePath, file);

                await EncryptFile(filePath, CryptographyFunction, encryptionKey);

                await _fileService.CreateFile(userID, filename, file.ContentType, type);

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                    FileShare.None, 4096, FileOptions.DeleteOnClose);

                return File(fileStream, file.ContentType, filename);
            }
            catch (FormatException ex)
            {
                return StatusCode(422, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    await _fileService.DeleteFile(filePath);
                    await _fileService.DeleteFile($"{filePath}.tmp");

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
                return StatusCode(500, new { message = "Unexpected error" });
            }
        }

        private byte[] CheckAndConvertKey(string key)
        {
            if (!Regex.IsMatch(key, Validation.EncryptionKey) || !_validation.IsBase64String(key))
                throw new FormatException(ErrorMessage.InvalidKey);

            return Convert.FromBase64String(key);
        }

        private async Task EncryptFile(string filePath, Func<string, byte[], CancellationToken, Task<CryptographyResult>> CryptographyFunction, byte[] key)
        {
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var cryptographyTask = CryptographyFunction(filePath, key, cancellationToken);
            var timeoutTask = Task.Delay(TASK_AWAITING);

            var completedTask = await Task.WhenAny(cryptographyTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                cts.Cancel();

                if (System.IO.File.Exists(filePath))
                    await Task.Run(() => System.IO.File.Delete(filePath));

                throw new InvalidOperationException(ErrorMessage.TaskTimedOut);
            }
            else
            {
                var cryptographyResult = await cryptographyTask;
                if (!cryptographyResult.Success)
                    throw new InvalidOperationException(ErrorMessage.BadCryptographyData);
            }
        }

        public async Task<CryptographyParams> GetCryptographyParams(string fileType)
        {
            string lowerFileType = fileType.ToLowerInvariant();

            try
            {
                if (lowerFileType == privateType)
                {
                    return new CryptographyParams(await _redisCache.CacheKey(_redisKeys.PrivateKey, _userInfo.UserId));
                }
                else if (lowerFileType == internalType)
                {
                    return new CryptographyParams(await _redisCache.CacheKey(_redisKeys.InternalKey, _userInfo.UserId));
                }
                else if (lowerFileType == receivedType)
                {
                    return new CryptographyParams(await _redisCache.CacheKey(_redisKeys.ReceivedKey, _userInfo.UserId));
                }
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
    }

    public record CryptographyParams(string EncryptionKey);
}
