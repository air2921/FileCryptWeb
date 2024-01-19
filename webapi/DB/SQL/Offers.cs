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
        private readonly IRead<UserModel> _readUser;

        public Offers(FileCryptDbContext dbContext, IRead<UserModel> readUser)
        {
            _dbContext = dbContext;
            _readUser = readUser;
        }

        public async Task Create(OfferModel offerModel)
        {
            var sender = await _readUser.ReadById(offerModel.sender_id, null);
            var receiver = await _readUser.ReadById(offerModel.receiver_id, null);
            bool bothExist = receiver is not null && sender is not null;
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
