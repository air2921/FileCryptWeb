using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Files.Mimes;
using webapi.Localization.English;

namespace webapi.DB.SQL.Files.Mimes
{
    public class ReadMime : IReadMime
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadMime(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> ReadMimeById(int id)
        {
            var mime = await _dbContext.Mimes.FindAsync(id) ??
                throw new MimeException(ExceptionMimeMessages.MimeNotFound);

            return mime.mime_name;
        }

        public async Task<HashSet<string>> ReadAllMimes()
        {
            var mimes = await _dbContext.Mimes.Select(m => m.mime_name).ToListAsync();

            var hashSetMimes = mimes.ToHashSet();

            return hashSetMimes;
        }
    }
}
