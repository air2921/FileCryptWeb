using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Links : ICreate<LinkModel>, IRead<LinkModel>, IDelete<LinkModel>, IDeleteByName<LinkModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public Links(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(LinkModel linkModel)
        {
            await _dbContext.AddAsync(linkModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<LinkModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.Links.FirstOrDefaultAsync(l => l.link_id == id) ??
                throw new LinkException(ExceptionLinkMessages.LinkNotFound);
        }

        public async Task<IEnumerable<LinkModel>> ReadAll(int? user_id, int skip, int count)
        {
            var query = _dbContext.Links
                .OrderByDescending(l => l.created_at)
                .AsQueryable();

            if (user_id.HasValue)
                return await query.Where(l => l.user_id == user_id).Skip(skip).Take(count).ToListAsync();

            return await query.Skip(skip).Take(count).ToListAsync();
        }

        public async Task DeleteById(int id, int? user_id)
        {
            var link = await _dbContext.Links.FirstOrDefaultAsync(l => l.link_id == id) ??
                throw new LinkException(ExceptionLinkMessages.LinkNotFound);

            _dbContext.Links.Remove(link);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string token, int? user_id)
        {
            var link = await _dbContext.Links.FirstOrDefaultAsync(l => l.u_token == token) ??
                throw new LinkException(ExceptionLinkMessages.LinkNotFound);

            _dbContext.Links.Remove(link);
            await _dbContext.SaveChangesAsync();
        }
    }
}
