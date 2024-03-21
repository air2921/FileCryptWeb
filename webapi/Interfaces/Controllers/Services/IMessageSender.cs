namespace webapi.Interfaces.Controllers.Services
{
    public interface IMessageSender
    {
        public Task SendMessage(string name, string email, object data);
    }
}
