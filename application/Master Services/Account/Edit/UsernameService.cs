﻿using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using System.Text.RegularExpressions;

namespace application.Master_Services.Account.Edit
{
    public class UsernameService(
        IRepository<UserModel> userRepository,
        IRedisCache redisCache)
    {
        public async Task<Response> UpdateUsername(string username, int id)
        {
            try
            {
                if (!Regex.IsMatch(username, RegularEx.Username))
                    return new Response { Status = 400, Message = Message.INVALID_FORMAT };

                var user = await userRepository.GetById(id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                user.username = username;
                await userRepository.Update(user);
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{id}");

                return new Response { Status = 200, Message = Message.UPDATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
