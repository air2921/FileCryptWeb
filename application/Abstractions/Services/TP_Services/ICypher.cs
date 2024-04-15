using application.DTO;

namespace application.Abstractions.Services.TP_Services
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(CryptographyDTO cryptoData);
    }
}
