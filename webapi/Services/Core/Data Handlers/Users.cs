using Newtonsoft.Json;
using webapi.Interfaces;
using webapi.Interfaces.Redis;
using webapi.Models;

namespace webapi.Services.Core.Data_Handlers
{
    public class Users(
        IRepository<UserModel> userRepository,
        IRedisCache redisCache,
        ILogger<Users> logger) : ICacheHandler<UserModel>
    {
        public async Task<UserModel> CacheAndGet(object dataObject)
        {
            try
            {
                var userObj = dataObject as UserObject ?? throw new FormatException();
                var user = new UserModel();
                var cache = await redisCache.GetCachedData(userObj.CacheKey);
                if (cache is null)
                {
                    user = await userRepository.GetById(userObj.UserId);
                    if (user is null)
                        return null;

                    user.password = string.Empty;
                    user.email = userObj.IsOwner ? user.email : string.Empty;

                    await redisCache.CacheData(userObj.CacheKey, user, TimeSpan.FromMinutes(10));
                    return user;
                }

                user = JsonConvert.DeserializeObject<UserModel>(cache);
                if (user is null)
                    throw new FormatException();

                user.email = userObj.IsOwner ? user.email : string.Empty;
                return user;
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

        public async Task<IEnumerable<UserModel>> CacheAndGetRange(object dataObject)
        {
            try
            {
                var userObj = dataObject as UserRangeObject ?? throw new FormatException();
                var users = new List<UserModel>();
                var cache = await redisCache.GetCachedData(userObj.CacheKey);
                if (cache is null)
                {
                    users = (List<UserModel>)await userRepository.GetAll(query => query.Where(u => u.username.Equals(userObj.Username)));
                    foreach (var user in users)
                    {
                        user.password = string.Empty;
                        user.email = string.Empty;
                    }

                    await redisCache.CacheData(userObj.CacheKey, users, TimeSpan.FromMinutes(5));
                    return users;
                }

                users = JsonConvert.DeserializeObject<List<UserModel>>(cache);
                if (users is not null)
                    return users;
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

    public record class UserObject(string CacheKey, int UserId, bool IsOwner);
    public record class UserRangeObject(string CacheKey, string Username);
}
