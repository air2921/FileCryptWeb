using application.Abstractions.Endpoints.Admin;
using application.Helper_Services;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace application.Master_Services.Admin
{
    public class Admin_UserService(
        [FromKeyedServices(ImplementationKey.ADMIN_USER_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ADMIN_USER_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository) : IAdminUserService
    {
        public async Task<Response> DeleteUser(int userId)
        {
            var target = await userRepository.GetById(userId);
            if (target is null)
                return new Response { Status = 404, Message = Message.NOT_FOUND };

            if (!validator.IsValid(target.role))
                return new Response { Status = 403, Message = Message.FORBIDDEN };

            await userRepository.Delete(userId);
            return new Response { Status = 204 };
        }

        public async Task<Response> BlockUser(int userId, bool block)
        {
            var target = await userRepository.GetById(userId);
            if (target is null)
                return new Response { Status = 404, Message = Message.NOT_FOUND };

            if (!validator.IsValid(target.role))
                return new Response { Status = 403, Message = Message.FORBIDDEN };

            await transaction.CreateTransaction(target, block);
            return new Response { Status = 200, Message = Message.UPDATED };
        }
    }
}
