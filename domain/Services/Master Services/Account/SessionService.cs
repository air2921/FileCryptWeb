using domain.Abstractions.Data;
using domain.Abstractions.Services;
using domain.DTO;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using domain.Services.Additional;
using domain.Specifications;
using Microsoft.Extensions.DependencyInjection;
using services.Abstractions;
using services.DTO;
using services.Exceptions;

namespace domain.Services.Master_Services.Account
{
    public class SessionService(
        ISessionHelper sessionHelper,
        [FromKeyedServices(ImplementationKey.ACCOUNT_SESSION_SERVICE)] IDataManagement dataManagament,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IPasswordManager passwordManager,
        webapi.Helpers.Abstractions.ITokenService tokenService,
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
                    return await sessionHelper.GenerateCredentials(user, tokenService.GenerateRefreshToken());

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
            catch (OperationCanceledException)
            {
                return new Response { Status = 500, Message = Message.ERROR };
            }
        }

        public async Task<Response> Verify2Fa(int code, string email, string refresh)
        {
            try
            {
                var userContext = (UserContextDTO)await dataManagament.GetData($"{USER_OBJECT}{email.ToLowerInvariant()}");
                if (userContext is null)
                    return new Response { Status = 404, Message = Message.TASK_TIMED_OUT };

                if (!passwordManager.CheckPassword(code.ToString(), userContext.Code))
                    return new Response { Status = 403, Message = Message.INCORRECT };

                var user = await userRepository.GetById(userContext.UserId);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                return await sessionHelper.GenerateCredentials(user, tokenService.GenerateRefreshToken());
            }
            catch (FormatException)
            {
                return new Response { Status = 500, Message = Message.ERROR };
            }
            catch (OperationCanceledException)
            {
                return new Response { Status = 500, Message = Message.ERROR };
            }
        }

        public Task<Response> Logout(string refresh) => sessionHelper.RevokeToken(refresh);
    }
}
