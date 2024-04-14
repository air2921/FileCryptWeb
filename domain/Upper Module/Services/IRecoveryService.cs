using domain.DTO;
using domain.Services;

namespace domain.Upper_Module.Services
{
    public interface IRecoveryService
    {
        public Task<Response> SendTicket(string email);
        public Task<Response> ChangePassword(RecoveryDTO dto);
    }
}
