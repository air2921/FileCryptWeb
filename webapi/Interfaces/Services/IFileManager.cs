namespace webapi.Interfaces.Services
{
    public interface IFileManager
    {
        HashSet<string> GetMimesFromCsvFile(string filePath);
    }
}
