namespace webapi.Interfaces.Services
{
    public interface IVirusCheck
    {
        public Task<bool> GetResultScan(IFormFile file);
    }
}
