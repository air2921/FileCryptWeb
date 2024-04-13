using domain.DTO;
using domain.Services;

namespace domain.Abstractions.Services
{
    public interface IRecoveryService
    {
        public Task<Response> SendTicket(string email);
        public Task<Response> ChangePassword(RecoveryDTO dto);
    }
}
