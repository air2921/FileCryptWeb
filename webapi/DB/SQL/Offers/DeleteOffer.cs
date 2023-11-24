using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Offers
{
    public class DeleteOffer : IDelete<OfferModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public DeleteOffer(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteById(int id)
        {
            var offer = await _dbContext.Offers.FindAsync(id) ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);

            _dbContext.Offers.Remove(offer);
            await _dbContext.SaveChangesAsync();
        }
    }
}
