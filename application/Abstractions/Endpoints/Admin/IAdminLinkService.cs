using application.Master_Services;

namespace application.Abstractions.Endpoints.Admin
{
    public interface IAdminLinkService
    {
        Task<Response> GetOne(int linkId);
        Task<Response> GetRange(int? userId, int skip, int count, bool byDesc, bool? expired);
        Task<Response> DeleteOne(int linkId);
        Task<Response> DeleteRange(IEnumerable<int> identifiers);
    }
}
