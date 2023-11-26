using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Offers;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Offers
{
    public class ReadOffer : IReadOffer
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadOffer(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OfferModel> ReadOneOffer(int id)
        {
            var offer = await _dbContext.Offers.FirstOrDefaultAsync(o=> o.offer_id == id) ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);

            return offer;
        }

        public async Task<List<OfferModel>> ReadAllReceivedOffers(int receiverId)
        {
            var offers = await _dbContext.Offers.Where(o => o.receiver_id == receiverId).ToListAsync() ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);

            return offers;
        }

        public async Task<List<OfferModel>> ReadAllSendedOffers(int senderId)
        {
            var offers = await _dbContext.Offers.Where(o => o.sender_id == senderId).ToListAsync() ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);

            return offers;
        }

        public async Task<List<OfferModel>> ReadAllOffers(OfferModel offerModel)
        {
            var receiverOffers = _dbContext.Offers
                .Where(o => o.receiver_id == offerModel.receiver_id);

            var senderOffers = _dbContext.Offers
                .Where(o => o.sender_id == offerModel.sender_id);

            var combinedOffers = await receiverOffers.Union(senderOffers).ToListAsync() ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);

            return combinedOffers;
        }
    }
}
