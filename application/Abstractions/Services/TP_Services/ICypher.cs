using application.DTO.Inner;

namespace application.Abstractions.Services.TP_Services
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(CryptographyDTO cryptoData);
    }
}
