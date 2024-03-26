using Microsoft.AspNetCore.Mvc;
using webapi.Attributes;
using webapi.Exceptions;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Core
{
    public class CryptographyService(
        IFileService fileService,
        ICryptographyHelper helper,
        IRedisKeys redisKeys,
        IUserInfo userInfo,
        ILogger<CryptographyService> logger) : ControllerBase, ICryptographyProvider
    {
        private const string DEFAULT_FOLDER = "C:\\FileCryptWeb";
        private readonly string privateType = FileType.Private.ToString().ToLowerInvariant();
        private readonly string internalType = FileType.Internal.ToString().ToLowerInvariant();
        private readonly string receivedType = FileType.Received.ToString().ToLowerInvariant();

        [NonAction]
        [Helper]
        public async Task<IActionResult> EncryptFile(CryptographyOperationOptions options)
        {
            var filename = Guid.NewGuid().ToString() + "_" + options.File.FileName;
            var filePath = Path.Combine(DEFAULT_FOLDER, filename);

            try
            {
                if (!fileService.CheckFileType(options.Type))
                    return StatusCode(400, new { message = "Invalid route request" });

                if (!Directory.Exists(DEFAULT_FOLDER))
                    Directory.CreateDirectory(DEFAULT_FOLDER);

                if (!fileService.CheckFile(options.File) || await fileService.IsProhibitedMIME(options.File.ContentType))
                    return StatusCode(422, new { message = Message.INVALID_FILE });

                var encryptionKey = helper.CheckAndConvertKey(options.Key);

                await fileService.UploadFile(filePath, options.File);
                await helper.EncryptFile(filePath, options.Operation, encryptionKey, options.UserID, options.Username);
                await fileService.CreateFile(
                    options.UserID,
                    filename,
                    options.File.ContentType,
                    fileService.GetFileCategory(options.File.ContentType),
                    options.Type);

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
                    logger.LogError(exc.ToString());
                    return StatusCode(422, new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return StatusCode(500, new { message = Message.ERROR });
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
                    return new CryptographyParams(await helper.CacheKey(redisKeys.PrivateKey, userInfo.UserId), isValidRoute);
                else if (lowerFileType == internalType)
                    return new CryptographyParams(await helper.CacheKey(redisKeys.InternalKey, userInfo.UserId), isValidRoute);
                else if (lowerFileType == receivedType)
                    return new CryptographyParams(await helper.CacheKey(redisKeys.ReceivedKey, userInfo.UserId), isValidRoute);
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
