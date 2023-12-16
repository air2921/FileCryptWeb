using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using webapi.Interfaces.Controllers;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Services;
using webapi.Services.Cryptography;

namespace webapi.Controllers.Base
{
    public class CryptographyControllerBase : ControllerBase, ICryptographyControllerBase
    {
        public const string DEFAULT_FOLDER = "C:\\FileCryptWeb";

        private readonly IFileService _fileService;
        private readonly ILogger<CryptographyControllerBase> _logger;
        private readonly IValidation _validation;

        public CryptographyControllerBase(IFileService fileService, ILogger<CryptographyControllerBase> logger, IValidation validation)
        {
            _fileService = fileService;
            _logger = logger;
            _validation = validation;
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

                    return StatusCode(400, new { message = ex.Message });
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc.ToString());
                    return StatusCode(400, new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(400, new { message = "Unexpected error" });
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
            try
            {
                using var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var cryptographyTask = CryptographyFunction(filePath, key, cancellationToken);
                var timeoutTask = Task.Delay(30000);

                var completedTask = await Task.WhenAny(cryptographyTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    cts.Cancel();

                    if (System.IO.File.Exists(filePath))
                        await Task.Run(() => System.IO.File.Delete(filePath));

                    throw new InvalidOperationException(ErrorMessage.TaskTimedOut);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());

                if (System.IO.File.Exists(filePath))
                    await Task.Run(() => System.IO.File.Delete(filePath));

                throw new InvalidOperationException("Unexpected error");
            }
        }
    }
}
