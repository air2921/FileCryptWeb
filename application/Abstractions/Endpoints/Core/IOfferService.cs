using application.Master_Services;

namespace application.Abstractions.Endpoints.Core
{
    public interface IOfferService
    {
        Task<Response> Add(int senderId, int receiverId, int keyId, int storageId, string code);
        Task<Response> Accept(int userId, string keyName, int offerId, int storageId, string code);
        Task<Response> GetOne(int userId, int offerId, bool bodyHide);
        Task<Response> GetRange(int userId, int skip, int count, bool byDesc,
            bool? sended, bool? isAccepted, string? type, bool bodyHide);
        Task<Response> DeleteOne(int userId, int offerId);
    }
}
