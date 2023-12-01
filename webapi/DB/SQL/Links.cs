using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Links : ICreate<LinkModel>, IRead<LinkModel>, IDelete<Links>, IDeleteByName<LinkModel>
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
                throw new LinkException("");
        }

        public async Task<IEnumerable<LinkModel>> ReadAll()
        {
            return await _dbContext.Links.ToListAsync() ??
                throw new LinkException("");
        }

        public async Task DeleteById(int id)
        {
            var link = await _dbContext.Links.FirstOrDefaultAsync(l => l.link_id == id) ??
                throw new LinkException(ExceptionMimeMessages.MimeNotFound);

            _dbContext.Links.Remove(link);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string token)
        {
            var link = await _dbContext.Links.FirstOrDefaultAsync(l => l.u_token == token) ??
                throw new LinkException(ExceptionMimeMessages.MimeNotFound);

            _dbContext.Links.Remove(link);
            await _dbContext.SaveChangesAsync();
        }
    }
}
