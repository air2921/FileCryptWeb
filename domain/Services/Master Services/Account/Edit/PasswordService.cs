using domain.Abstractions.Data;
using domain.Abstractions.Services;
using domain.DTO;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using services.Abstractions;
using System.Text.RegularExpressions;

namespace domain.Services.Master_Services.Account.Edit
{
    internal class PasswordService(
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] IDataManagement dataManagament,
        IRepository<UserModel> userRepository,
        IPasswordManager passwordManager) : IPasswordService
    {
        public async Task<Response> UpdatePassword(PasswordDTO dto, int id)
        {
            try
            {
                if (!Regex.IsMatch(dto.NewPassword, Validation.Password))
                    return new Response { Status = 422, Message = Message.INVALID_FORMAT };

                var user = await userRepository.GetById(id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!passwordManager.CheckPassword(dto.OldPassword, user.password))
                    return new Response { Status = 401, Message = Message.INCORRECT };

                await transaction.CreateTransaction(user, dto.NewPassword);
                await dataManagament.DeleteData(id);

                return new Response { Status = 200, Message = Message.UPDATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
