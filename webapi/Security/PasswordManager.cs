using webapi.Interfaces.Services;
using BC = BCrypt.Net;

namespace webapi.Security
{
    public class PasswordManager : IPasswordManager
    {
        private readonly ILogger<PasswordManager> _logger;

        public PasswordManager(ILogger<PasswordManager> logger)
        {
            _logger = logger;
        }

        public string HashingPassword(string password)
        {
            return BC.BCrypt.EnhancedHashPassword(password, BC.HashType.SHA512);
        }

        public bool CheckPassword(string InputPassword, string CorrectPassword)
        {
            try
            {
                return BC.BCrypt.EnhancedVerify(InputPassword, CorrectPassword, BC.HashType.SHA512);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());

                return false;
            }
        }
    }
}
