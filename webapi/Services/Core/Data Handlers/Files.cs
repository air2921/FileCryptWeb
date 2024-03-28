using Newtonsoft.Json;
using webapi.DB;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace webapi.Services.Core.Data_Handlers
{
    public class Files(
        IRepository<FileModel> fileRepository,
        ISorting sorting,
        IRedisCache redisCache,
        ILogger<Files> logger) : ICacheHandler<FileModel>
    {
        public async Task<FileModel> CacheAndGet(object dataObject)
        {
            try
            {
                var fileObj = dataObject as FileObject ?? throw new FormatException();
                var file = new FileModel();

                var cache = await redisCache.GetCachedData(fileObj.CacheKey);
                if (cache is null)
                {
                    file = await fileRepository.GetByFilter(query => query
                        .Where(f => f.user_id.Equals(fileObj.UserId) && f.file_id.Equals(fileObj.FileId)));

                    if (file is null)
                        return null;

                    await redisCache.CacheData(fileObj.CacheKey, file, TimeSpan.FromMinutes(5));
                    return file;
                }

                file = JsonConvert.DeserializeObject<FileModel>(cache);
                if (file is not null)
                    return file;
                else
                    return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Files));
                throw new FormatException();
            }
        }

        public async Task<IEnumerable<FileModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var fileObj = dataObject as FileRangeObject ?? throw new FormatException();
                var files = new List<FileModel>();

                var cache = await redisCache.GetCachedData(fileObj.CacheKey);
                if (cache is null)
                {
                    files = (List<FileModel>)await fileRepository
                        .GetAll(sorting.SortFiles(fileObj.UserId, fileObj.Skip, fileObj.Count, fileObj.ByDesc, fileObj.Type, fileObj.Mime, fileObj.Category));

                    await redisCache.CacheData(fileObj.CacheKey, files, TimeSpan.FromMinutes(5));
                    return files;
                }

                files = JsonConvert.DeserializeObject<List<FileModel>>(cache);
                if (files is not null)
                    return files;
                else
                    throw new FormatException();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Files));
                throw new FormatException();
            }
        }
    }

    public record class FileObject(string CacheKey, int UserId, int FileId);
    public record class FileRangeObject(string CacheKey, int UserId, int Skip, int Count, bool ByDesc, string? Type, string? Category, string? Mime);
}
