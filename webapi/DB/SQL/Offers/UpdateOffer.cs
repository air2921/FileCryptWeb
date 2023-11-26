using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Offers;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Offers
{
    public class UpdateOffer : IUpdateOffer
    {
        private readonly FileCryptDbContext _dbContext;

        public UpdateOffer(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateReceivedKeyFromOffer(OfferModel offerModel)
        {
            var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.offer_id == offerModel.offer_id) ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);

            if ((bool)offer.is_accepted)
                throw new OfferException(ExceptionOfferMessages.OfferIsAccepted);

            var targetUser = await _dbContext.Keys.FirstOrDefaultAsync(k => k.user_id == offer.receiver_id) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            targetUser.received_internal_key = offer.offer_body;
            await _dbContext.SaveChangesAsync();
        }
    }
}
