﻿using application.DTO.Outer;
using application.Helpers;
using application.Helpers.Localization;
using application.Helper_Services;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using application.Abstractions.TP_Services;

namespace application.Master_Services.Account.Edit
{
    public class PasswordService(
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_PASSWORD_SERVICE)] IDataManagement dataManagament,
        IRepository<UserModel> userRepository,
        IHashUtility hashUtility)
    {
        public async Task<Response> UpdatePassword(PasswordDTO dto, int id)
        {
            try
            {
                if (!Regex.IsMatch(dto.NewPassword, RegularEx.Password))
                    return new Response { Status = 422, Message = Message.INVALID_FORMAT };

                var user = await userRepository.GetById(id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!hashUtility.Verify(dto.OldPassword, user.password))
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
