using application.DTO.Outer;
using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface ICryptographyService
    {
        Task<Response> Cypher(CypherFileDTO dto);
    }
}
