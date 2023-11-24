using webapi.Models;

namespace webapi.Interfaces.SQL.Offers
{
    public interface IUpdateOffer
    {
        Task UpdateReceivedKeyFromOffer(OfferModel offerModel);
    }
}
