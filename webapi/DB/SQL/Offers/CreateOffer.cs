using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Offers
{
    public class CreateOffer : ICreate<OfferModel>
    {
        private readonly FileCryptDbContext _dbContext;
        public CreateOffer(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(OfferModel offerModel)
        {
            bool exists = await _dbContext.Users.AnyAsync(u => u.id == offerModel.sender_id && u.id == offerModel.receiver_id);
            if (!exists)
                throw new UserException(AccountErrorMessage.UserNotFound);

            await _dbContext.AddAsync(offerModel);
            await _dbContext.SaveChangesAsync();
        }
    }
}
