using webapi.Models;

namespace webapi.Interfaces.Services
{
    public interface IFileManager
    {
        public string GetReactAppUrl();
        public void AddMimeCollection(ref HashSet<FileMimeModel> mimeModels, HashSet<string> existingMimes);
    }
}
