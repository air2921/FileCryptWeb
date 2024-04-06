using webapi.DTO;

namespace webapi.Third_Party_Services.Abstractions
{
    public interface IEmailSender
    {
        public Task SendMessage(EmailDto email);
    }
}
