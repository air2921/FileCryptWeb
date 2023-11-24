using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Files;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Files
{
    public class ReadFile : IReadFile
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadFile(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public const string FILE_NAME = "fileName";
        public const string FILE_ID = "fileID";

        public async Task<FileModel> ReadFileByIdOrName(FileModel fileModel, string searchField)
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

            return await query.FirstOrDefaultAsync() ??
                throw new FileException(ExceptionFileMessages.FileNotFound);
        }

        public async Task<int[]> ReadFilesIDByUserID(FileModel fileModel)
        {
            var files = await _dbContext.Files
                .Where(f => f.user_id == fileModel.user_id)
                .Select(f => f.file_id)
                .ToListAsync() ?? throw new FileException(ExceptionFileMessages.NoOneFileNotFound);

            return files.ToArray();
        }

        public async Task<List<FileModel>> ReadAllFiles(FileModel fileModel)
        {
            return await _dbContext.Files.Where(f => f.user_id == fileModel.user_id).ToListAsync() ??
                throw new FileException(ExceptionFileMessages.NoOneFileNotFound);
        }
    }
}
