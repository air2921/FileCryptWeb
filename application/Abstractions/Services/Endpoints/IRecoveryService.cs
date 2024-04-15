using application.DTO;
using application.Services;
using domain.DTO;

namespace application.Abstractions.Services.Endpoints
{
    public interface IRecoveryService
    {
        public Task<Response> SendTicket(string email);
        public Task<Response> ChangePassword(RecoveryDTO dto);
    }
}
