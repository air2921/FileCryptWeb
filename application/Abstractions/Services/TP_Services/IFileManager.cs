namespace application.Abstractions.Services.TP_Services
{
    public interface IFileManager
    {
        public string GetReactAppUrl();
        public IEnumerable<string> AddMimeCollection(HashSet<string> existingMimes);
    }
}
