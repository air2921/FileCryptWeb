using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface IStorageService
    {
        Task<Response> Add(string storageName, string accessCode, string? description, int userId);
        Task<Response> GetOne(int storageId, int userId);
        Task<Response> GetRange(int userId, int skip, int count, bool byDesc);
        Task<Response> DeleteOne(int userId, int storageId, string accessCode);
    }
}
