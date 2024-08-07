﻿using application.Abstractions.TP_Services;
using application.DTO.Inner;
using application.Helper_Services;
using application.Helpers;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace application.Master_Services.Account.Edit
{
    public class _2FaService(
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IValidator validator,
        IEmailSender emailSender,
        IRepository<UserModel> userRepository,
        IHashUtility hashUtility,
        IGenerate generate)
    {
        private readonly string CODE = $"_2FaController_VerificationCode#";

        public async Task<Response> SendVerificationCode(string password, int id)
        {
            try
            {
                var user = await userRepository.GetById(id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!hashUtility.Verify(password, user.password))
                    return new Response { Status = 401, Message = Message.INCORRECT };

                int code = generate.GenerateCode(6);
                await emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.Change2FaHeader,
                    message = EmailMessage.Change2FaBody + code
                });

                await dataManagament.SetData($"{CODE}{id}", code);

                return new Response { Status = 200, Message = Message.EMAIL_SENT };
            }
            catch (SmtpClientException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> UpdateState(int code, bool enable, int id)
        {
            try
            {
                if (!validator.IsValid(await dataManagament.GetData($"{CODE}{id}"), code))
                    return new Response { Status = 401, Message = Message.INCORRECT };

                var user = await userRepository.GetById(id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await transaction.CreateTransaction(user, enable);
                await dataManagament.DeleteData(id);

                return new Response { Status = 200, Message = Message.UPDATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 404, Message = ex.Message };
            }
        }
    }
}
