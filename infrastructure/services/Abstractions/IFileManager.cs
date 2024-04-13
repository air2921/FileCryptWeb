namespace services.Abstractions
{
    public interface IFileManager
    {
        public string GetReactAppUrl();
        public IEnumerable<string> AddMimeCollection(HashSet<string> existingMimes);
    }
}
