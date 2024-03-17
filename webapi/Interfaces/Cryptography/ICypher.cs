using webapi.Cryptography;

namespace webapi.Interfaces.Cryptography
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(CryptographyData cryptoData);
    }
}
