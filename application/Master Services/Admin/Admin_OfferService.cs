using application.Abstractions.Endpoints.Admin;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.Sorting_Specifications;

namespace application.Master_Services.Admin
{
    public class Admin_OfferService(
        IRepository<OfferModel> repository,
        IRedisCache redisCache) : IAdminOfferService
    {
        public async Task<Response> GetOne(int offerId)
        {
            try
            {
                var offer = await repository.GetById(offerId);
                if (offer is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                return new Response { Status = 200, ObjectData = offer };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> GetRange(int? userId, int skip, int count, bool byDesc,
            bool? sended, bool? isAccepted, string? type)
        {
            try
            {
                return new Response
                {
                    Status = 200,
                    ObjectData = await repository
                            .GetAll(new OffersSortSpec(userId, skip, count, byDesc, sended, isAccepted, type))
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> DeleteOne(int offerId)
        {
            try
            {
                var offer = await repository.Delete(offerId);
                if (offer is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.sender_id}");
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.OFFERS_PREFIX}{offer.receiver_id}");

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
                var offers = await repository.DeleteMany(identifiers);
                await redisCache.DeleteRedisCache(offers, ImmutableData.OFFERS_PREFIX, item => item.sender_id);
                await redisCache.DeleteRedisCache(offers, ImmutableData.OFFERS_PREFIX, item => item.receiver_id);

                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
