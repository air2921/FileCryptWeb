using application.Master_Services;

namespace application.Abstractions.Endpoints.Account
{
    public interface IEmailService
    {
        public Task<Response> StartEmailChangeProcess(string password, int id);
        public Task<Response> ConfirmOldEmail(string email, int code, string username, int id);
        public Task<Response> ConfirmNewEmailAndUpdate(int code, int id);
    }
}
