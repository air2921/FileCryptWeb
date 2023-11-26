using webapi.Models;

namespace webapi.Interfaces.SQL.Files
{
    public interface IReadFile
    {
        Task<FileModel> ReadFileByIdOrName(FileModel fileModel, string searchField);
        Task<List<FileModel>> ReadAllFiles(FileModel fileModel);
    }
}
