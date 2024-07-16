using domain.Specifications.By_Relation_Specifications;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using application.Helpers.Localization;
using application.Helpers;
using application.Cache_Handlers;

namespace application.Master_Services.Core
{
    public class FilesService(
        IRepository<FileModel> repository,
        ICacheHandler<FileModel> cacheHandler,
        IRedisCache redisCache)
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
    }
}
