using domain.Abstractions.Data;
using domain.Abstractions.Services;
using domain.Exceptions;
using domain.Helpers;
using domain.Localization;
using domain.Models;
using domain.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using services.Abstractions;
using services.DTO;
using services.Exceptions;

namespace domain.Services.Master_Services.Account.Edit
{
    public class _2FaService(
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] ITransaction<UserModel> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_2FA_SERVICE)] IValidator validator,
        IEmailSender emailSender,
        IRepository<UserModel> userRepository,
        IPasswordManager passwordManager,
        IGenerate generate) : I2FaService
    {
        private readonly string CODE = $"_2FaController_VerificationCode#";

        public async Task<Response> SendVerificationCode(string password, int id)
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

                return new Response { Status = 200 };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 404, Message = ex.Message };
            }
        }
    }
}
