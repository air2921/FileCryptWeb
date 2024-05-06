using services.Cryptography.Abstractions;
using System.Security.Cryptography;

namespace services.Cryptography
{
    public class AesCreator : IAes
    {
        public Aes GetAesInstance()
        {
            return Aes.Create();
        }
    }
}
