using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface IFileService
    {
        Task<Response> GetOne(int userId, int fileId);
        Task<Response> GetRange(int userId, int skip, int count, bool byDesc, string? category, string? mime);
        Task<Response> GetRangeForInterval(bool byDesc, DateTime start, DateTime end, int userId);
        Task<Response> DeleteOne(int userId, int fileId);
    }
}
