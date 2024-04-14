using domain.Services;

namespace domain.Upper_Module.Services
{
    public interface IEmailService
    {
        public Task<Response> StartEmailChangeProcess(string password, int id);
        public Task<Response> ConfirmOldEmail(string email, int code, string username, int id);
        public Task<Response> ConfirmNewEmailAndUpdate(int code, int id);
    }
}
