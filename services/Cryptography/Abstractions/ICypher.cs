using services.DTO;

namespace services.Cryptography.Abstractions
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(CryptographyDTO cryptoData);
    }
}
