

using domain.DTO;

namespace domain.Abstractions.Services
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(CryptographyDTO cryptoData);
    }
}
