using webapi.Models;

namespace webapi.Interfaces.SQL.Files
{
    public interface IDeleteFile
    {
        Task DeleteFileByNameOrID(FileModel fileModel, string searchField);
        Task DeleteAllUserFiles(int userId);
    }
}
