namespace webapi.Third_Party_Services.Abstractions
{
    public interface IVirusCheck
    {
        public Task<bool> GetResultScan(IFormFile file, CancellationToken cancellationToken);
    }
}
