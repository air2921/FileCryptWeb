using webapi.Cryptography;

namespace webapi.Interfaces.Cryptography
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(string filePath, byte[] key, CancellationToken cancellationToken, string operation);
    }
}
