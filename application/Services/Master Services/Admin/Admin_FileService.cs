using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.Sorting_Specifications;

namespace application.Services.Master_Services.Admin
{
    public class Admin_FileService(
        IRepository<FileModel> repository,
        IRedisCache redisCache)
    {
        public async Task<Response> GetOne(int fileId)
        {
            try
            {
                var file = await repository.GetById(fileId);
                if (file is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
                    return new Response { Status = 200, ObjectData = new { file } };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRangeFiles(int? userId, int skip, int count, bool byDesc, string? category)
        {
            try
            {
                return new Response
                {
                    Status = 200,
                    ObjectData = new
                    {
                        files = await repository
                            .GetAll(new FilesSortSpec(userId, skip, count, byDesc, null, category))
                    }
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int fileId)
        {
            try
            {
                var file = await repository.Delete(fileId);
                if (file is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.FILES_PREFIX}{file.user_id}");
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteRange(IEnumerable<int> identifiers)
        {
            try
            {
                var fileList = await repository.DeleteMany(identifiers);
                await redisCache.DeleteRedisCache(fileList, ImmutableData.FILES_PREFIX, item => item.user_id);
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
