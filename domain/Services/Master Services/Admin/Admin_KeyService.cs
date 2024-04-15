using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;

namespace domain.Services.Master_Services.Admin
{
    public class Admin_KeyService(IRepository<KeyModel> repository, IRedisCache redisCache)
    {
        public async Task<Response> RevokeReceivcedKey(int userId)
        {
            try
            {
                var keys = await repository.GetByFilter(new KeysByRelationSpec(userId));
                if (keys is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                keys.received_key = null;
                await repository.Update(keys);

                await redisCache.DeleteCache($"{ImmutableData.RECEIVED_KEY}{userId}");
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.KEYS_PREFIX}{userId}");

                return new Response { Status = 200, Message = Message.REMOVED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
