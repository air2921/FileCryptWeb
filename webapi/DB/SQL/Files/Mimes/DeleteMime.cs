using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Files.Mimes
{
    public class DeleteMime : IDelete<FileMimeModel>, IDeleteByName<FileMimeModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public DeleteMime(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteById(int id)
        {
            var mime = await _dbContext.Mimes.FindAsync(id) ??
                throw new MimeException(ExceptionMimeMessages.MimeNotFound);

            _dbContext.Mimes.Remove(mime);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteByName(string mime_name)
        {
            var mime = await _dbContext.Mimes.FirstOrDefaultAsync(m => m.mime_name == mime_name) ??
                throw new MimeException(ExceptionMimeMessages.MimeNotFound);

            _dbContext.Mimes.Remove(mime);
            await _dbContext.SaveChangesAsync();
        }
    }
}
