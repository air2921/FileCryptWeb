using webapi.Models;

namespace webapi.Interfaces.SQL.Offers
{
    public interface IReadOffer
    {
        Task<OfferModel> ReadOneOffer(int id);
        Task<List<OfferModel>> ReadAllReceivedOffers(int receiverId);
        Task<List<OfferModel>> ReadAllSendedOffers(int senderId);
        Task<List<OfferModel>> ReadAllOffers(OfferModel offerModel);
    }
}
