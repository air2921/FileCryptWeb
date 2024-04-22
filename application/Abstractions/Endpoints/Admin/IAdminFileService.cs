using application.Master_Services;

namespace application.Abstractions.Endpoints.Admin
{
    public interface IAdminFileService
    {
        Task<Response> GetOne(int fileId);
        Task<Response> GetRange(int? userId, int skip, int count, bool byDesc, string? category);
        Task<Response> DeleteOne(int fileId);
        Task<Response> DeleteRange(IEnumerable<int> identifiers);
    }
}
