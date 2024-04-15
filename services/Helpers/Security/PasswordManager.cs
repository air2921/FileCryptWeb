using Microsoft.Extensions.Logging;
using BC = BCrypt.Net;
using application.Abstractions.Services.TP_Services;

namespace services.Helpers.Security
{
    public class PasswordManager(ILogger<PasswordManager> logger) : IPasswordManager
    {
        public string HashingPassword(string password)
        {
            return BC.BCrypt.EnhancedHashPassword(password, BC.HashType.SHA512);
        }

        public bool CheckPassword(string input, string src)
        {
            try
            {
                return BC.BCrypt.EnhancedVerify(input, src, BC.HashType.SHA512);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());

                return false;
            }
        }
    }
}
