using Newtonsoft.Json;
using webapi.Exceptions;
using webapi.Helpers;
using webapi.Interfaces.Controllers.Services;
using webapi.Interfaces.Redis;
using webapi.Interfaces.Services;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Services.Core
{
    public class FileService(
        IRepository<FileModel> fileRepository,
        IRepository<FileMimeModel> mimeRepository,
        IGetSize getSize,
#pragma warning disable CS9113 // Параметр не прочитан.
        IVirusCheck virusCheck,
#pragma warning restore CS9113 // Параметр не прочитан.
        IRedisCache redisCache,
        ILogger<FileService> logger) : IFileService
    {
        private readonly string privateType = FileType.Private.ToString().ToLowerInvariant();
        private readonly string internalType = FileType.Internal.ToString().ToLowerInvariant();
        private readonly string receivedType = FileType.Received.ToString().ToLowerInvariant();
        private const int TASK_AWAITING = 10000;

        public bool CheckFileType(string type)
        {
            string lowerType = type.ToLowerInvariant();

            string[] typesArray =
            {
                privateType,
                internalType,
                receivedType
            };

            return typesArray.Contains(lowerType);
        }

        public bool CheckFile(IFormFile file)
        {
            try
            {
                if (file.Length == 0 || file.ContentType is null)
                    return false;

                if (getSize.GetFileSizeInMb(file) > 75)
                    return false;

                // !!!!!
                // If you are a country where ClamAV is blocked, leave this block of code commented out and make method asynchronous
                // !!!!!

                //using var cts = new CancellationTokenSource();
                //var cancellationToken = cts.Token;

                //var virusCheckTask = virusCheck.GetResultScan(file, cancellationToken);
                //var timeoutTask = Task.Delay(TASK_AWAITING);
                //var completedTask = await Task.WhenAny(virusCheckTask, timeoutTask);

                //if (completedTask == timeoutTask)
                //{
                //    cts.Cancel();
                //    logger.LogCritical("Virus check task was cancelled", nameof(CheckFile));
                //    return false;
                //}

                //if (!await virusCheckTask)
                //    return false;

                return true;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString(), nameof(CheckFile));
                return false;
            }
        }

        public async Task<bool> IsProhibitedMIME(string mime)
        {
            var cacheMimes = await redisCache.GetCachedData(ImmutableData.MIME_COLLECTION);
            if (cacheMimes is not null)
            {
                string[] mimesArray = JsonConvert.DeserializeObject<string[]>(cacheMimes);
                if (mimesArray is null || mimesArray.Length.Equals(0))
                    return false;

                return mimesArray.Contains(mime);
            }
            else
            {
                string[] dbMimes = (await mimeRepository.GetAll()).Select(m => m.mime_name).ToArray();
                await redisCache.CacheData(ImmutableData.MIME_COLLECTION, dbMimes, TimeSpan.FromDays(3));

                return dbMimes.Contains(mime);
            }
        }

        public async Task UploadFile(string filePath, IFormFile file)
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public string GetFileCategory(string contentType)
        {
            return contentType.Split('/')[0] switch
            {
                "application" => "application",
                "audio" => "audio",
                "font" => "font",
                "image" => "image",
                "message" => "message",
                "model" => "model",
                "multipart" => "multipart",
                "text" => "text",
                "video" => "video",
                _ => throw new ArgumentException("Invalid MIME type"),
            };
        }

        public async Task CreateFile(int userID, string uniqueFileName, string mime, string mimeCategory, string fileType)
        {
            try
            {
                await fileRepository.Add(new FileModel
                {
                    user_id = userID,
                    file_name = uniqueFileName,
                    file_mime = mime,
                    file_mime_category = mimeCategory,
                    operation_date = DateTime.UtcNow,
                    type = fileType,
                });
            }
            catch (EntityNotCreatedException)
            {
                throw;
            }
        }
    }
}
