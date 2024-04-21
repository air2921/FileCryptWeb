namespace application.Abstractions.TP_Services
{
    public interface IFileManager
    {
        public string GetReactAppUrl();
        public HashSet<string> AddMimeCollection(HashSet<string> existingMimes);
    }
}
