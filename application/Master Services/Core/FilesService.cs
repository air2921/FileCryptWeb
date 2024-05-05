using domain.Specifications.By_Relation_Specifications;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using application.Helpers.Localization;
using application.Helpers;
using application.Cache_Handlers;
using application.Abstractions.Endpoints.Core;
using domain.Specifications.Sorting_Specifications;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace application.Master_Services.Core
{
    internal class FilesService(
        IRepository<FileModel> repository,
        ICacheHandler<FileModel> cacheHandler,
        IRedisCache redisCache) : IFileService
    {
        public async Task<Response> GetOne(int userId, int fileId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.FILES_PREFIX}{userId}_{fileId}";
                var file = await cacheHandler.CacheAndGet(new FileObject(cacheKey, userId, fileId));
                if (file is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
                    return new Response { Status = 200, ObjectData = file };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRange(int userId, int skip, int count,
            bool byDesc, string? category, string? mime)
        {
            try
            {
                var cacheKey = $"{ImmutableData.FILES_PREFIX}{userId}_{skip}_{count}_{byDesc}_{category}_{mime}";
                return new Response
                {
                    Status = 200,
                    ObjectData = await cacheHandler.CacheAndGetRange(
                        new FileRangeObject(cacheKey, userId, skip, count, byDesc, mime, category))
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (FormatException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRangeForInterval(bool byDesc, DateTime start, DateTime end, int userId)
        {
            try
            {
                var files = new List<FileModel>();
                var cacheKey = $"{ImmutableData.FILES_PREFIX}{userId}_{byDesc}_{start:dd.MM.yyyy}_{end:dd.MM.yyyy}";
                var cache = await redisCache.GetCachedData(cacheKey);
                if (cache is null)
                {
                    files = (List<FileModel>)await repository.GetAll(new FilesForIntervalSpec(userId, byDesc, start, end));
                    await redisCache.CacheData(cacheKey, files, TimeSpan.FromMinutes(30));
                    return new Response { Status = 200, ObjectData = GetCountInDay(files) };
                }

                files = JsonConvert.DeserializeObject<List<FileModel>>(cache);
                if (files is not null)
                    return new Response { Status = 200, ObjectData = GetCountInDay(files) };
                else
                    return new Response { Status = 500, Message = Message.ERROR };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (JsonException)
            {
                return new Response { Status = 500, Message = Message.ERROR };
            }
        }

        public async Task<Response> DeleteOne(int userId, int fileId)
        {
            try
            {
                var file = await repository.DeleteByFilter(new FileByIdAndRelationSpec(fileId, userId));
                if (file is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{userId}");
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        private HashSet<Day> GetCountInDay(IEnumerable<FileModel> files)
        {
            var dict = files
                .GroupBy(file => file.operation_date.ToString("dd.MM.yyyy"))
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            var activity = new HashSet<Day>();
            foreach (var el in dict)
                activity.Add(new Day { Date = el.Key, ActivityCount = el.Value });

            return activity;
        }

        public class Day
        {
            public string Date { get; set; }
            public int ActivityCount { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj is null || obj is not Day other)
                    return false;

                return Date == other.Date && ActivityCount == other.ActivityCount;
            }

            public override int GetHashCode() => HashCode.Combine(Date, ActivityCount);
        }
    }
}
