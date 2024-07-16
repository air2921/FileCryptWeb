using application.DTO.Inner;
using application.DTO.Outer;
using application.Helpers;
using application.Helper_Services;
using application.Helpers.Localization;
using domain.Abstractions.Data;
using domain.Exceptions;
using domain.Models;
using domain.Specifications;
using Microsoft.Extensions.DependencyInjection;
using application.Helper_Services.Account;
using application.Abstractions.Inner;
using application.Abstractions.TP_Services;

namespace application.Master_Services.Account
{
    public class SessionService(
        ISessionHelper sessionHelper,
        [FromKeyedServices(ImplementationKey.ACCOUNT_SESSION_SERVICE)] IDataManagement dataManagament,
        IRepository<TokenModel> tokenRepository,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IHashUtility hashUtility,
        IGenerate generate,
        ITokenComparator tokenComparator)
    {
        private readonly string USER_OBJECT = "AuthSessionController_UserObject_Email:";

        public async Task<Response> Login(LoginDTO dto)
        {
            try
            {
                var user = await userRepository.GetByFilter(new UserByEmailSpec(dto.Email.ToLowerInvariant()));
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (user.is_blocked)
                    return new Response { Status = 404, Message = Message.BLOCKED };

                if (!hashUtility.Verify(dto.Password, user.password))
                    return new Response { Status = 404, Message = Message.INCORRECT };

                if (!user.is_2fa_enabled)
                    return await sessionHelper.GenerateCredentials(user, tokenComparator.CreateRefresh());

                int code = generate.GenerateCode(6);
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
                    Code = hashUtility.Hash(code.ToString())
                });

                return new Response { Status = 200, Message = Message.EMAIL_SENT, ObjectData = true };
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

        public async Task<Response> Verify2Fa(int code, string email)
        {
            try
            {
                if (await dataManagament.GetData($"{USER_OBJECT}{email.ToLowerInvariant()}") is not UserContextDTO userContext)
                    return new Response { Status = 404, Message = Message.TASK_TIMED_OUT };

                if (!hashUtility.Verify(code.ToString(), userContext.Code))
                    return new Response { Status = 403, Message = Message.INCORRECT };

                var user = await userRepository.GetById(userContext.UserId);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                return await sessionHelper.GenerateCredentials(user, tokenComparator.CreateRefresh());
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

        public async Task<Response> Logout(string refresh)
        {
            try
            {
                return await sessionHelper.RevokeToken(refresh);
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> UpdateJwt(string refresh)
        {
            try
            {
                var token = await tokenRepository.GetByFilter(new RefreshTokenByTokenSpec(refresh));
                if (token is null || token.expiry_date < DateTime.UtcNow)
                    return new Response { Status = 401, Message = Message.UNAUTHORIZED };

                var user = await userRepository.GetById(token.user_id);
                if (user is null || user.is_blocked)
                    return new Response { Status = 401, Message = Message.UNAUTHORIZED };

                return new Response
                {
                    Status = 201,
                    ObjectData = tokenComparator.CreateJWT(new JwtDTO
                    {
                        UserId = user.id,
                        Email = user.email,
                        Role = user.role,
                        Username = user.username,
                        Expires = ImmutableData.JwtExpiry
                    })
                };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
