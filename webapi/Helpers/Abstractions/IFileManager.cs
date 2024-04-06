using webapi.Models;

namespace webapi.Helpers.Abstractions
{
    public interface IFileManager
    {
        public string GetReactAppUrl();
        public void AddMimeCollection(ref HashSet<FileMimeModel> mimeModels, HashSet<string> existingMimes);
    }
}
