using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Keys;
using webapi.Localization.English;

namespace webapi.DB.SQL.Keys
{
    public class ReadKeys : IReadKeys
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadKeys(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> ReadPersonalInternalKey(int userid)
        {
            var user = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == userid) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            if (user.person_internal_key != null)
            {
                return user.person_internal_key;
            }
            throw new KeyException(ExceptionKeyMessages.InternalKeyNotFound);
        }

        public async Task<string> ReadReceivedInternalKey(int userid)
        {
            var user = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == userid) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            if (user.received_internal_key != null)
            {
                return user.received_internal_key;
            }
            throw new KeyException(ExceptionKeyMessages.ReceivedKeyNotFound);
        }

        public async Task<string> ReadPrivateKey(int userid)
        {
            var user = await _dbContext.Keys.FirstOrDefaultAsync(u => u.user_id == userid) ??
                throw new UserException(ExceptionUserMessages.UserNotFound);

            return user.private_key;
        }
    }
}
