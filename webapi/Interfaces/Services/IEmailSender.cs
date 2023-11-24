namespace webapi.Interfaces.Services
{
    public interface IEmailSender<TModel>
    {
        public Task SendMessage(TModel user, string messageHeader, string message);
    }
}
