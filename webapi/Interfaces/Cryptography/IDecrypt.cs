using webapi.Cryptography;

namespace webapi.Interfaces.Cryptography
{
    public interface IDecrypt
    {
        public Task<CryptographyResult> DecryptFileAsync(string filePath, byte[] key, CancellationToken cancellationToken);
    }
}
