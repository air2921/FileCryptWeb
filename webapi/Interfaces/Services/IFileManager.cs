namespace webapi.Interfaces.Services
{
    public interface IFileManager
    {
        HashSet<string> GetMimesFromCsvFile(string filePath);
        public string GetReactAppUrl(string path, bool isChrome);
    }
}
