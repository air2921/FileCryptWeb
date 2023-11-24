using webapi.Models;

namespace webapi.Interfaces.SQL.Files
{
    public interface IReadFile
    {
        Task<FileModel> ReadFileByIdOrName(FileModel fileModel, string searchField);
        Task<int[]> ReadFilesIDByUserID(FileModel fileModel);
        Task<List<FileModel>> ReadAllFiles(FileModel fileModel);
    }
}
