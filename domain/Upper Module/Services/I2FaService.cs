using domain.Services;

namespace domain.Upper_Module.Services
{
    public interface I2FaService
    {
        public Task<Response> SendVerificationCode(string password, int id);
        public Task<Response> UpdateState(int code, bool enable, int id);
    }
}
