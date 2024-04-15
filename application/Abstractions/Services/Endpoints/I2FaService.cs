using application.Services;

namespace application.Abstractions.Services.Endpoints
{
    public interface I2FaService
    {
        public Task<Response> SendVerificationCode(string password, int id);
        public Task<Response> UpdateState(int code, bool enable, int id);
    }
}
