using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface IStorageItemService
    {
        Task<Response> Add(int userId, int storageId, string code, string name, string value);
        Task<Response> GetOne(int userId, int storageId, int keyId, string code);
        Task<Response> GetRange(int userId, int storageId, int skip, int count, bool byDesc, string code);
        Task<Response> DeleteOne(int userId, int storageId, int keyId, string code);
    }
}
