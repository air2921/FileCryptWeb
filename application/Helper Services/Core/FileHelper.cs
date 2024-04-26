using application.Abstractions.Inner;
using application.Abstractions.TP_Services;
using application.Helpers;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace application.Helper_Services.Core
{
    public class FileHelper(
        IRepository<FileModel> fileRepository,
        IRepository<MimeModel> mimeRepository, 
#pragma warning disable CS9113 // Параметр не прочитан.
        IVirusCheck virusCheck,
#pragma warning restore CS9113 // Параметр не прочитан.
        IGetSize getSize,
        IRedisCache redisCache,
        ILogger<FileHelper> logger) : IFileHelper
    {
        private const int TASK_AWAITING = 10000;

        public bool IsValidFile(Stream stream, string contentType)
        {
            try
            {
                if (stream.Length == 0 || contentType is null)
                    return false;

                if (getSize.GetFileSizeInMb(stream) > 75)
                    return false;

                // !!!!!
                // If you are a country where ClamAV is blocked, leave this block of code commented out and make method asynchronous
                // !!!!!

                //using var cts = new CancellationTokenSource();
                //var cancellationToken = cts.Token;

                //var virusCheckTask = virusCheck.GetResultScan(stream, cancellationToken);
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
                logger.LogCritical(ex.ToString(), nameof(IsValidFile));
                return false;
            }
        }

        public async Task<bool> IsAllowedMIME(string mime)
        {
            try
            {
                var cacheMimes = await redisCache.GetCachedData(ImmutableData.MIME_COLLECTION);
                if (cacheMimes is not null)
                {
                    string[]? mimesArray = JsonConvert.DeserializeObject<string[]>(cacheMimes);
                    if (mimesArray is null || mimesArray.Length.Equals(0))
                        return false;

                    if (mimesArray.Contains(mime))
                        return false;
                    else
                        return true;
                }
                else
                {
                    string[] dbMimes = (await mimeRepository.GetAll()).Select(m => m.mime_name).ToArray();
                    await redisCache.CacheData(ImmutableData.MIME_COLLECTION, dbMimes, TimeSpan.FromDays(1));

                    if (dbMimes.Contains(mime))
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
                return false;
            }
        }

        public async Task UploadFile(string filePath, Stream outFile)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await outFile.CopyToAsync(fileStream);
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

        public async Task CreateFile(int userID, string uniqueFileName, string mime, string mimeCategory)
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
                });
            }
            catch (EntityException)
            {
                throw;
            }
        }
    }
}
