using System.Security.Cryptography;

namespace webapi.Cryptography.Abstractions
{
    public interface IAes
    {
        public Aes GetAesInstance();
    }
}
