using application.Abstractions.Services.Endpoints;
using application.Abstractions.Services.TP_Services;
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

namespace application.Master_Services.Account
{
    public class RecoveryService(
        IRecoveryHelper recoveryHelper,
        [FromKeyedServices(ImplementationKey.ACCOUNT_RECOVERY_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IRepository<LinkModel> linkRepository,
        IEmailSender emailSender,
        IRedisCache redisCache,
        IFileManager fileManager,
        IGenerate generate) : IRecoveryService
    {
        public async Task<Response> SendTicket(string email)
        {
            try
            {
                var user = await userRepository.GetByFilter(new UserByEmailSpec(email.ToLowerInvariant()));
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString() + generate.GenerateKey();
                await recoveryHelper.CreateTokenTransaction(user, token);
                await emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = email,
                    subject = EmailMessage.RecoveryAccountHeader,
                    message = EmailMessage.RecoveryAccountBody + $"{fileManager.GetReactAppUrl()}/auth/recovery?token={token}"
                });

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");

                return new Response { Status = 201, Message = Message.EMAIL_SENT };
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

        public async Task<Response> ChangePassword(RecoveryDTO dto)
        {
            try
            {
                if (!validator.IsValid(dto.Password))
                    return new Response { Status = 400, Message = Message.INVALID_FORMAT };

                var link = await linkRepository.GetByFilter(new RecoveryTokenByTokenSpec(dto.Token));
                if (link is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (link.expiry_date < DateTime.UtcNow)
                {
                    await linkRepository.Delete(link.link_id);
                    return new Response { Status = 422, Message = Message.FORBIDDEN };
                }

                var user = await userRepository.GetById(link.user_id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await recoveryHelper.RecoveryTransaction(user, dto.Token, dto.Password);

                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.NOTIFICATIONS_PREFIX}{user.id}");
                await redisCache.DeteteCacheByKeyPattern($"{ImmutableData.USER_DATA_PREFIX}{user.id}");

                return new Response { Status = 200 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
