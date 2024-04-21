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
using application.Abstractions.TP_Services;
using application.Abstractions.Endpoints.Account;

namespace application.Master_Services.Account
{
    public class RegistrationService(
        [FromKeyedServices(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE)] ITransaction<UserDTO> transaction,
        [FromKeyedServices(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE)] IDataManagement dataManagament,
        [FromKeyedServices(ImplementationKey.ACCOUNT_REGISTRATION_SERVICE)] IValidator validator,
        IRepository<UserModel> userRepository,
        IEmailSender emailSender,
        IHashUtility hashUtility,
        IGenerate generate) : IRegistrationService
    {
        private readonly string USER_OBJECT = "AuthRegistrationController_UserObject_Email:";

        public async Task<Response> Registration(RegisterDTO dto)
        {
            try
            {
                dto.Email = dto.Email.ToLowerInvariant();
                int code = generate.GenerateSixDigitCode();

                if (!validator.IsValid(dto))
                    return new Response { Status = 400, Message = Message.INVALID_FORMAT };

                var user = await userRepository.GetByFilter(new UserByEmailSpec(dto.Email));
                if (user is not null)
                    return new Response { Status = 400, Message = Message.USER_EXISTS };

                await emailSender.SendMessage(new EmailDto
                {
                    username = dto.Username,
                    email = dto.Email,
                    subject = EmailMessage.VerifyEmailHeader,
                    message = EmailMessage.VerifyEmailBody + code
                });

                await dataManagament.SetData($"{USER_OBJECT}{dto.Email}", new UserDTO
                {
                    Email = dto.Email,
                    Password = dto.Password,
                    Username = dto.Username,
                    Role = Role.User.ToString(),
                    Flag2Fa = dto.Is_2fa_enabled,
                    Code = code.ToString()
                });

                return new Response(true) { Status = 200, Message = Message.EMAIL_SENT };
            }
            catch (SmtpClientException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (ArgumentException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }

        public async Task<Response> VerifyAccount(int code, string email)
        {
            try
            {
                if (await dataManagament.GetData($"{USER_OBJECT}{email.ToLowerInvariant()}") is not UserDTO user)
                    return new Response { Status = 404, Message = Message.TASK_TIMED_OUT };

                if (!hashUtility.Verify(code.ToString(), user.Code))
                    return new Response { Status = 422, Message = Message.INCORRECT };

                await transaction.CreateTransaction(user);

                return new Response(true) { Status = 200, Message = Message.CREATED };
            }
            catch (EntityException ex)
            {
                return new Response { Status = 500, Message = ex.Message };
            }
        }
    }
}
