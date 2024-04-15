﻿using domain.Abstractions.Data;
using domain.Abstractions.Services;
using domain.DTO;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using domain.Services.Additional.Account;
using domain.Specifications;
using domain.Upper_Module.Services;
using Microsoft.Extensions.DependencyInjection;

namespace domain.Services.Master_Services.Account
{
    public class SessionService(
        ISessionHelper sessionHelper,
        [FromKeyedServices(ImplementationKey.ACCOUNT_SESSION_SERVICE)] IDataManagement dataManagament,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IPasswordManager passwordManager,
        IGenerate generate) : ISessionService
    {
        private readonly string USER_OBJECT = "AuthSessionController_UserObject_Email:";

        public async Task<Response> Login(LoginDTO dto, string refresh)
        {
            try
            {
                var user = await userRepository.GetByFilter(new UserByEmailSpec(dto.Email.ToLowerInvariant()));
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (user.is_blocked)
                    return new Response { Status = 404, Message = Message.BLOCKED };

                if (!passwordManager.CheckPassword(dto.Password, user.password))
                    return new Response { Status = 404, Message = Message.INCORRECT };

                if (!user.is_2fa_enabled)
                    return await sessionHelper.GenerateCredentials(user);

                int code = generate.GenerateSixDigitCode();
                await emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.Verify2FaHeader,
                    message = EmailMessage.Verify2FaBody + code
                });
                await dataManagament.SetData($"{USER_OBJECT}{user.email}", new UserContextDTO
                {
                    UserId = user.id,
                    Code = passwordManager.HashingPassword(code.ToString())
                });

                return new Response { Status = 200, Message = Message.EMAIL_SENT, ObjectData = new { confirm = true } };
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

        public async Task<Response> Verify2Fa(int code, string email, string refresh)
        {
            try
            {
                if (await dataManagament.GetData($"{USER_OBJECT}{email.ToLowerInvariant()}") is not UserContextDTO userContext)
                    return new Response { Status = 404, Message = Message.TASK_TIMED_OUT };

                if (!passwordManager.CheckPassword(code.ToString(), userContext.Code))
                    return new Response { Status = 403, Message = Message.INCORRECT };

                var user = await userRepository.GetById(userContext.UserId);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                return await sessionHelper.GenerateCredentials(user);
            }
            catch (FormatException)
            {
                return new Response { Status = 500, Message = Message.ERROR };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public Task<Response> Logout(string refresh) => sessionHelper.RevokeToken(refresh);
    }
}
