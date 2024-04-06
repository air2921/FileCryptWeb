using System.Security.Cryptography;
using webapi.Cryptography.Abstractions;

namespace webapi.Cryptography
{
    public class AesCreator : IAes
    {
        public Aes GetAesInstance()
        {
            return Aes.Create();
        }
    }
}
