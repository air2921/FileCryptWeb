using Microsoft.Extensions.Logging;
using BC = BCrypt.Net;
using application.Abstractions.TP_Services;

namespace services.Helpers.Security
{
    public class HashUtility(ILogger<HashUtility> logger) : IHashUtility
    {
        public string Hash(string password)
        {
            return BC.BCrypt.EnhancedHashPassword(password, BC.HashType.SHA512);
        }

        public bool Verify(string input, string src)
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
