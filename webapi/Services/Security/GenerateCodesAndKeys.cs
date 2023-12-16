using System.Security.Cryptography;
using webapi.Interfaces.Services;

namespace webapi.Services.Security
{
    public class GenerateCodesAndKeys : IGenerateSixDigitCode, IGenerateKey
    {
        private readonly Random _random = new();

        public int GenerateSixDigitCode()
        {
            return _random.Next(100000, 999999);
        }

        public string GenerateKey()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] key = new byte[32];
            rng.GetBytes(key);

            return Convert.ToBase64String(key);
        }
    }
}
