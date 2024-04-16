using application.Abstractions.Services.TP_Services;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.Sorting_Specifications;

namespace application.Services.Master_Services.Admin
{
    public class Admin_MimeService(
        IRepository<FileMimeModel> repository,
        IRedisCache redisCache,
        IFileManager fileManager)
    {
        public async Task<Response> GetOne(int mimeId)
        {
            try
            {
                var mime = await repository.GetById(mimeId);
                if (mime is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
                    return new Response { Status = 200, ObjectData = new { mime } };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRange(int skip, int count)
        {
            try
            {
                return new Response
                {
                    Status = 200,
                    ObjectData = new
                    {
                        mimes = await repository.GetAll(new MimesSortSpec(skip, count))
                    }
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> CreateOne(FileMimeModel model)
        {
            try
            {
                await repository.Add(model);
                await ClearCache();

                return new Response { Status = 201, Message = Message.CREATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> CreateRange(IEnumerable<FileMimeModel> models)
        {
            try
            {
                await repository.AddRange(models);
                await ClearCache();

                return new Response { Status = 201, Message = Message.CREATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> CreateRangeTemplate()
        {
            try
            {

                var mimes = fileManager.AddMimeCollection((await repository.GetAll()).Select(m => m.mime_name).ToHashSet());

                var mimeModels = new HashSet<FileMimeModel>();
                foreach (var mime in mimes)
                    mimeModels.Add(new FileMimeModel { mime_name = mime });

                await repository.AddRange(mimeModels);
                await ClearCache();

                return new Response { Status = 201, Message = Message.CREATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int mimeId)
        {
            try
            {
                var mime = await repository.Delete(mimeId);
                if (mime is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                
                await ClearCache();
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

                await repository.DeleteMany(identifiers);
                await ClearCache();

                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        private async Task ClearCache() => await redisCache.DeleteCache(ImmutableData.MIME_COLLECTION);
    }
}
