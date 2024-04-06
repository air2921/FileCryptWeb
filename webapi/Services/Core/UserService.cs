using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using webapi.Attributes;
using webapi.DB.Abstractions;
using webapi.DB.Ef;
using webapi.Helpers.Abstractions;
using webapi.Localization;
using webapi.Models;

namespace webapi.Services.Core
{
    public interface IUserHelpers
    {
        Task<IEnumerable<FileModel>> GetFiles(int userId);
        Task<IEnumerable<OfferModel>> GetOffers(int userId);
        Task<UserKeysObject> GetUserAndKeys(int userId);
    }

    public class UserService(
        FileCryptDbContext dbContext,
        IRedisCache redisCache,
        IRepository<FileModel> fileRepository,
        IRepository<OfferModel> offerRepository,
        IUserInfo userInfo) : IUserHelpers
    {
        [Helper]
        public async Task<IEnumerable<FileModel>> GetFiles(int userId)
        {
            var cacheKeyFiles = $"Profile_Files_{userId}";

            var cacheFiles = await redisCache.GetCachedData(cacheKeyFiles);
            if (cacheFiles is null)
            {
                var filesDb = await fileRepository.GetAll
                    (query => query.Where(f => f.user_id.Equals(userId)).OrderByDescending(f => f.operation_date).Skip(0).Take(5));

                await redisCache.CacheData(cacheKeyFiles, filesDb, TimeSpan.FromMinutes(1));

                return filesDb;
            }
            else
                return JsonConvert.DeserializeObject<List<FileModel>>(cacheFiles);
        }

        [Helper]
        public async Task<IEnumerable<OfferModel>> GetOffers(int userId)
        {
            var cacheKeyOffers = $"Profile_Offers_{userId}";
            var offers = new List<OfferModel>();

            var cacheOffers = await redisCache.GetCachedData(cacheKeyOffers);
            if (cacheOffers is null)
            {
                var offersDb = await offerRepository.GetAll
                    (query => query.Where(o => o.receiver_id.Equals(userId) || o.sender_id.Equals(userId))
                    .OrderByDescending(o => o.created_at).Skip(0).Take(5));

                await redisCache.CacheData(cacheKeyOffers, offersDb, TimeSpan.FromMinutes(1));

                offers = (List<OfferModel>)offersDb;
            }
            else
                offers = JsonConvert.DeserializeObject<List<OfferModel>>(cacheOffers);

            foreach (var offer in offers)
            {
                offer.offer_header = string.Empty;
                offer.offer_body = string.Empty;
            }

            return offers;
        }

        [Helper]
        public async Task<UserKeysObject> GetUserAndKeys(int userId)
        {
            var userAndKeys = await dbContext.Users
            .Where(u => u.id == userId)
                .Join(dbContext.Keys, user => user.id, keys => keys.user_id, (user, keys) => new { user, keys })
                .FirstOrDefaultAsync();

            if (userAndKeys is null)
                throw new ArgumentNullException(Message.NOT_FOUND);

            bool IsOwner = userId.Equals(userInfo.UserId);
            bool privateKey = userAndKeys.keys.private_key is not null;
            bool internalKey = userAndKeys.keys.internal_key is not null;
            bool receivedKey = userAndKeys.keys.received_key is not null;

            var user = new
            {
                id = userAndKeys.user.id,
                username = userAndKeys.user.username,
                email = IsOwner ? userAndKeys.user.email : null,
                role = userAndKeys.user.role,
                is_blocked = userAndKeys.user.is_blocked
            };

            var keys = new
            {
                privateKey,
                internalKey,
                receivedKey
            };

            return new UserKeysObject { user = user, isOwner = IsOwner, keys = keys };
        }
    }

    [AuxiliaryObject]
    public class UserKeysObject
    {
        public object user { get; set; }
        public object keys { get; set; }
        public bool isOwner { get; set; }
    }
}
