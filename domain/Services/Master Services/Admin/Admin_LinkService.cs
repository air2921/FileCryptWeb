using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Localization;
using domain.Models;
using domain.Specifications.Sorting_Specifications;

namespace domain.Services.Master_Services.Admin
{
    public class Admin_LinkService(IRepository<LinkModel> repository)
    {
        public async Task<Response> GetOne(int linkId)
        {
            try
            {
                var link = await repository.GetById(linkId);
                if (link is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
                    return new Response { Status = 200, ObjectData = new { link } };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRangeLinks(int? userId, int skip, int count, bool byDesc, bool? expired)
        {
            try
            {
                return new Response
                {
                    Status = 200,
                    ObjectData = new
                    {
                        links = await repository.GetAll(new LinksSortSpec(userId, skip, count, byDesc, expired))
                    }
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int linkId)
        {
            try
            {
                var link = await repository.Delete(linkId);
                if (link is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };
                else
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
                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
