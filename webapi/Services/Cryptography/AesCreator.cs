using System.Security.Cryptography;
using webapi.Interfaces.Cryptography;

namespace webapi.Services.Cryptography
{
    public class AesCreator : IAes
    {
        public Aes GetAesInstance()
        {
            return Aes.Create();
        }
    }
}
