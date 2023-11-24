using webapi.Models;

namespace webapi.Interfaces.SQL.Files
{
    public interface IUpdateFile
    {
        Task UpdateFileByNameOrID(FileModel fileModel, string searchField);
    }
}
