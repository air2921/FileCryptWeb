﻿using application.Helpers;
using application.Helpers.Localization;
using application.Cache_Handlers;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;

namespace application.Master_Services.Core
{
    public class UsersService(
        IRepository<UserModel> repository,
        ICacheHandler<UserModel> cacheHandler,
        IRedisCache redisCache)
    {
        public async Task<Response> GetOne(int ownerId, int targetId)
        {
            try
            {
                var cacheKey = $"{ImmutableData.USER_DATA_PREFIX}{targetId}";
                var user = await cacheHandler.CacheAndGet(new UserObject(cacheKey, targetId, ownerId == targetId));
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                return new Response { Status = 200, ObjectData = user };
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

        public async Task<Response> GetRange(string username, int skip, int count)
        {
            try
            {
                var cacheKey = $"{ImmutableData.USER_LIST}{username}_";

                return new Response
                {
                    Status = 200,
                    ObjectData = await cacheHandler.CacheAndGetRange(new UserRangeObject(cacheKey, username, skip, count))
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

        public async Task<Response> DeleteOne(int userId)
        {
            try
            {
                var user = await repository.Delete(userId);
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{user.id}");
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_LIST}{user.username}");

                return new Response { Status = 204 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
