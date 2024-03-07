using webapi.DTO;

namespace webapi.Interfaces.Services
{
    public interface IEmailSender
    {
        public Task SendMessage(EmailDto email);
    }
}
