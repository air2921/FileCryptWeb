using System.Security.Cryptography;

namespace services.Cryptography.Abstractions
{
    public interface IAes
    {
        public Aes GetAesInstance();
    }
}
