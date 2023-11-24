using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Files;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Files
{
    public class DeleteFile : IDeleteFile
    {
        private readonly FileCryptDbContext _dbContext;

        public DeleteFile(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public const string FILE_NAME = "fileName";
        public const string FILE_ID = "fileID";

        public async Task DeleteFileByNameOrID(FileModel fileModel, string searchField)
        {
            var query = _dbContext.Files.Where(f => f.user_id == fileModel.user_id);

            switch (searchField)
            {
                case FILE_ID:
                    query = query.Where(f => f.file_id == fileModel.file_id);
                    break;
                case FILE_NAME:
                    query = query.Where(f => f.file_name == fileModel.file_name);
                    break;
                default:
                    throw new ArgumentException("Invalid search field", nameof(searchField));
            }

            var file = await query.FirstOrDefaultAsync() ??
                throw new FileException(ExceptionFileMessages.FileNotFound);

            _dbContext.Remove(file);
            await _dbContext.SaveChangesAsync();
        }
    }
}
