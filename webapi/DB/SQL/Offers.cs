using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Offers : ICreate<OfferModel>, IDelete<OfferModel>, IRead<OfferModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public Offers(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(OfferModel offerModel)
        {
            var users = await _dbContext.Users.Select(u => u.id).ToArrayAsync();
            bool bothExist = users.Contains(offerModel.sender_id) && users.Contains(offerModel.receiver_id);
            if (!bothExist)
                throw new UserException(AccountErrorMessage.UserNotFound);

            await _dbContext.AddAsync(offerModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteById(int id, int? user_id)
        {
            var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.offer_id == id && (o.sender_id == user_id || o.receiver_id == user_id)) ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);

            _dbContext.Offers.Remove(offer);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<OfferModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.Offers.FirstOrDefaultAsync(o => o.offer_id == id) ??
                throw new OfferException(ExceptionOfferMessages.OfferNotFound);
        }

        public async Task<IEnumerable<OfferModel>> ReadAll(int? user_id, int skip, int count)
        {
            var query = _dbContext.Offers
                .OrderByDescending(o => o.created_at)
                .AsQueryable();

            if (user_id.HasValue)
                return await query.Where(o => o.sender_id == user_id || o.receiver_id == user_id).Skip(skip).Take(count).ToListAsync();

            return await query.Skip(skip).Take(count).ToListAsync();
        }
    }
}
