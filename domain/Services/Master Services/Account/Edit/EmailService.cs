using domain.Abstractions.Data;
using domain.Abstractions.Services;
using domain.DTO;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using domain.Specifications;
using domain.Upper_Module.Services;
using Microsoft.Extensions.DependencyInjection;

namespace domain.Services.Master_Services.Account.Edit
{
    public class EmailService(
        [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_EMAIL_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IPasswordManager passwordManager,
        IGenerate generate) : IEmailService
    {
        private readonly string EMAIL = $"EmailController_Email#";
        private readonly string OLD_EMAIL_CODE = $"EmailController_ConfirmationCode_OldEmail#";
        private readonly string NEW_EMAIL_CODE = $"EmailController_ConfirmationCode_NewEmail#";

        public async Task<Response> StartEmailChangeProcess(string password, int id)
        {
            try
            {
                var user = await userRepository.GetById(id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                if (!passwordManager.CheckPassword(password, user.password))
                    return new Response { Status = 401, Message = Message.INCORRECT };

                int code = generate.GenerateSixDigitCode();
                await emailSender.SendMessage(new EmailDto
                {
                    username = user.username,
                    email = user.email,
                    subject = EmailMessage.ConfirmOldEmailHeader,
                    message = EmailMessage.ConfirmOldEmailBody + code
                });

                await dataManagament.SetData($"{OLD_EMAIL_CODE}{id}", code);

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

        public async Task<Response> ConfirmOldEmail(string email, int code, string username, int id)
        {
            try
            {
                email = email.ToLowerInvariant();

                if (!validator.IsValid(await dataManagament.GetData($"{OLD_EMAIL_CODE}{id}"), code))
                    return new Response { Status = 400, Message = Message.INCORRECT };

                var user = await userRepository.GetByFilter(new UserByEmailSpec(email));
                if (user is not null)
                    return new Response { Status = 401, Message = Message.CONFLICT };

                int confirmationCode = generate.GenerateSixDigitCode();
                await emailSender.SendMessage(new EmailDto()
                {
                    username = username,
                    email = email,
                    subject = EmailMessage.ConfirmNewEmailHeader,
                    message = EmailMessage.ConfirmNewEmailBody + confirmationCode
                });

                await dataManagament.SetData($"{NEW_EMAIL_CODE}{id}", confirmationCode);
                await dataManagament.SetData($"{EMAIL}{id}", email);

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

        public async Task<Response> ConfirmNewEmailAndUpdate(int code, int id)
        {
            try
            {
                string? email = await dataManagament.GetData($"{EMAIL}{id}") as string;

                if (email is null || !validator.IsValid(await dataManagament.GetData(NEW_EMAIL_CODE), code))
                    return new Response { Status = 400, Message = Message.INCORRECT };

                var user = await userRepository.GetById(id);
                if (user is null)
                    return new Response { Status = 404, Message = Message.NOT_FOUND };

                await transaction.CreateTransaction(user, email);
                await dataManagament.DeleteData(id);

                return new Response { Status = 200 };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
