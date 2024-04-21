using application.Abstractions.Endpoints.Admin;
using application.Helper_Services;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace application.Master_Services.Admin
{
    public class Admin_TokenService(
        [FromKeyedServices(ImplementationKey.ADMIN_TOKEN_SERVICE)] ITransaction<TokenModel> transaction,
        [FromKeyedServices(ImplementationKey.ADMIN_TOKEN_SERVICE)] IValidator validator,
        IRepository<TokenModel> tokenRepository,
        IRepository<UserModel> userRepository) : IAdminTokenService
    {
        public async Task<Response> RevokeAllUserTokens(int targetId, string ownRole)
        {
            try
            {
                var target = await userRepository.GetById(targetId);
                if (target is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!validator.IsValid(target.role, ownRole))
                    return new Response { Status = 403, Message = Message.FORBIDDEN };

                await transaction.CreateTransaction(null, target.id);
                return new Response { Status = 200, Message = Message.REMOVED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> RevokeToken(int tokenId, string ownRole)
        {
            try
            {
                var token = await tokenRepository.GetById(tokenId);
                if (token is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                var target = await userRepository.GetById(token.user_id);
                if (target is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!validator.IsValid(target.role, ownRole))
                    return new Response { Status = 403, Message = Message.FORBIDDEN };

                await tokenRepository.Delete(tokenId);
                return new Response { Status = 200, Message = Message.REMOVED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
