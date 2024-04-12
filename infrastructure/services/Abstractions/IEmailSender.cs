using services.Sender.DTO;

namespace services.Abstractions
{
    public interface IEmailSender
    {
        public Task SendMessage(EmailDto email);
    }
}
