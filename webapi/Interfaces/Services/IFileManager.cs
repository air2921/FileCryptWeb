namespace webapi.Interfaces.Services
{
    public interface IFileManager
    {
        public string[] GetCsvFiles();
        public HashSet<string> GetMimesFromCsvFile(string filePath);
        public string GetReactAppUrl(string path, bool isChrome);
    }
}
