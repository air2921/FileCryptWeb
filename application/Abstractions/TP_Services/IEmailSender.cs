using application.DTO.Inner;

namespace application.Abstractions.TP_Services
{
    public interface IEmailSender
    {
        public Task SendMessage(EmailDto email);
    }
}
