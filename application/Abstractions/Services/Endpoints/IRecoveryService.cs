using application.DTO.Outer;
using application.Services;

namespace application.Abstractions.Services.Endpoints
{
    public interface IRecoveryService
    {
        public Task<Response> SendTicket(string email);
        public Task<Response> ChangePassword(RecoveryDTO dto);
    }
}
