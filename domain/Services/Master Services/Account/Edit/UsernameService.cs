using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Upper_Module.Services;
using System.Text.RegularExpressions;
using webapi.Models;

namespace domain.Services.Master_Services.Account.Edit
{
    public class UsernameService(
        IRepository<UserModel> userRepository,
        IRedisCache redisCache) : IUsernameService
    {
        public async Task<Response> UpdateUsername(string username, int id)
        {
            try
            {
                if (!Regex.IsMatch(username, Validation.Username))
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
            catch (UnauthorizedAccessException)
            {
                throw;
            }
        }
    }
}
