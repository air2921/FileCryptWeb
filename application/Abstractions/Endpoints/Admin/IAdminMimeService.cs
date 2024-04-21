using application.Master_Services;

namespace application.Abstractions.Endpoints.Admin
{
    public interface IAdminMimeService
    {
        Task<Response> GetOne(int mimeId);
        Task<Response> GetRange(int skip, int count);
        Task<Response> CreateOne(string mime);
        Task<Response> CreateRange(IEnumerable<string> mimes);
        Task<Response> CreateRangeTemplate();
        Task<Response> DeleteOne(int mimeId);
        Task<Response> DeleteRange(IEnumerable<int> identifiers);
    }
}
