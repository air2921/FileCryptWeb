namespace webapi.Interfaces.SQL.Files.Mimes
{
    public interface IReadMime
    {
        Task<string> ReadMimeById(int id);
        Task<HashSet<string>> ReadAllMimes();
    }
}
