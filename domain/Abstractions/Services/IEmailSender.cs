using domain.DTO;

namespace domain.Abstractions.Services
{
    public interface IEmailSender
    {
        public Task SendMessage(EmailDto email);
    }
}
