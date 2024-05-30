namespace application.Abstractions.TP_Services
{
    public interface IS3Manager
    {
        Task Upload(Stream stream, string key);
        Task<Stream> Download(string key);
        Task<Dictionary<string, Stream>> DownloadCollection(IEnumerable<string> keys);
        Task Delete(string key);
    }
}
