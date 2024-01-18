using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Files : ICreate<FileModel>, IRead<FileModel>, IDelete<FileModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public Files(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(FileModel fileModel)
        {
            await _dbContext.AddAsync(fileModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<FileModel> ReadById(int id, bool? byForeign)
        {
            return await _dbContext.Files.FirstOrDefaultAsync(f => f.file_id == id) ??
                throw new FileException(ExceptionFileMessages.FileNotFound);
        }

        public async Task<IEnumerable<FileModel>> ReadAll(int? user_id, int skip, int count)
        {
            var query = _dbContext.Files
                .OrderByDescending(f => f.operation_date)
                .AsQueryable();

            if (user_id.HasValue)
                return await query.Where(f => f.user_id == user_id).Skip(skip).Take(count).ToListAsync();

            return await query.Skip(skip).Take(count).ToListAsync();
        }

        public async Task DeleteById(int id, int? user_id)
        {
            var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.file_id == id && f.user_id == user_id) ??
                throw new FileException(ExceptionFileMessages.FileNotFound);

            _dbContext.Remove(file);
            await _dbContext.SaveChangesAsync();
        }
    }
}
