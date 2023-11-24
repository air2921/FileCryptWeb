using System.Security.Cryptography;

namespace webapi.Interfaces.Cryptography
{
    public interface IAes
    {
        public Aes GetAesInstance();
    }
}
