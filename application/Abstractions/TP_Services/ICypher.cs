using application.DTO.Inner;

namespace application.Abstractions.TP_Services
{
    public interface ICypher
    {
        Task<CryptographyResult> CypherFileAsync(CryptographyDTO cryptoData);
    }
}
