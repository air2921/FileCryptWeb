using webapi.Cryptography;

namespace webapi.Cryptography.Abstractions
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(CryptographyData cryptoData);
    }
}
