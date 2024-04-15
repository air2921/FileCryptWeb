using application.DTO;

namespace application.Abstractions.Services.TP_Services
{
    public interface IEmailSender
    {
        public Task SendMessage(EmailDto email);
    }
}
