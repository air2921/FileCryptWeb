namespace application.Abstractions.TP_Services
{
    public interface IS3Manager
    {
        Task Upload(Stream stream, string key);
        Task<Stream> Download(string key);
        Task Delete(string key);
    }
}
