using webapi.Cryptography;

namespace webapi.Interfaces.Cryptography
{
    public interface IEncrypt
    {
        public Task<CryptographyResult> EncryptFileAsync(string filePath, byte[] key, CancellationToken cancellationToken);
    }
}
