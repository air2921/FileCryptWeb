using application.Master_Services;

namespace application.Abstractions.Endpoints.Admin
{
    public interface IAdminOfferService
    {
        Task<Response> GetOne(int offerId);
        Task<Response> GetRange(int? userId, int skip, int count, bool byDesc, bool? sent, bool? isAccepted, int? type);
        Task<Response> DeleteOne(int offerId);
        Task<Response> DeleteRange(IEnumerable<int> identifiers);
    }
}
