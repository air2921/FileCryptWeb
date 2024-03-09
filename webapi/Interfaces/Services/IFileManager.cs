using webapi.Models;

namespace webapi.Interfaces.Services
{
    public interface IFileManager
    {
        public HashSet<string> GetMimesFromCsvFile(string filePath);
        public string GetReactAppUrl(string path, bool isChrome);
        public void AddSecureCollection(ref HashSet<FileMimeModel> mimeModels, HashSet<string> existingMimes);
        public void AddFullCollection(ref HashSet<FileMimeModel> mimeModels, HashSet<string> existingMimes);
    }
}
