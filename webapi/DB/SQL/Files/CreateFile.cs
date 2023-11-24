using webapi.Interfaces.SQL;
using webapi.Models;

namespace webapi.DB.SQL.Files
{
    public class CreateFile : ICreate<FileModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public CreateFile(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Create(FileModel fileModel)
        {
            await _dbContext.AddAsync(fileModel);
            await _dbContext.SaveChangesAsync();
        }
    }
}
