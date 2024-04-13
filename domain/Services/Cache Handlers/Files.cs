using domain;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using domain.Specifications.By_Relation_Specifications;
using domain.Specifications.Sorting_Specifications;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace domain.Services
{
    public class Files(
        IRepository<FileModel> fileRepository,
        IRedisCache redisCache,
        ILogger<Files> logger) : ICacheHandler<FileModel>
    {
        public async Task<FileModel> CacheAndGet(object dataObject)
        {
            try
            {
                var fileObj = dataObject as FileObject ?? throw new FormatException(Message.ERROR);
                var file = new FileModel();

                var cache = await redisCache.GetCachedData(fileObj.CacheKey);
                if (cache is null)
                {
                    file = await fileRepository.GetByFilter(new FileByIdAndRelationSpec(fileObj.FileId, fileObj.UserId));

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
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Files));
                throw new FormatException(Message.ERROR);
            }
        }

        public async Task<IEnumerable<FileModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var fileObj = dataObject as FileRangeObject ?? throw new FormatException(Message.ERROR);
                var files = new List<FileModel>();

                var cache = await redisCache.GetCachedData(fileObj.CacheKey);
                if (cache is null)
                {
                    files = (List<FileModel>)await fileRepository
                        .GetAll(new FilesSortSpec(fileObj.UserId, fileObj.Skip, fileObj.Count, fileObj.ByDesc, fileObj.Type, fileObj.Mime, fileObj.Category));

                    await redisCache.CacheData(fileObj.CacheKey, files, TimeSpan.FromMinutes(5));
                    return files;
                }

                files = JsonConvert.DeserializeObject<List<FileModel>>(cache);
                if (files is not null)
                    return files;
                else
                    throw new FormatException(Message.ERROR);
            }
            catch (EntityException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogCritical(ex.ToString(), nameof(Files));
                throw new FormatException(Message.ERROR);
            }
        }
    }

    public record class FileObject(string CacheKey, int UserId, int FileId);
    public record class FileRangeObject(string CacheKey, int UserId, int Skip, int Count, bool ByDesc, string? Type, string? Category, string? Mime);
}
