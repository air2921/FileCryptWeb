namespace domain.Abstractions.Services
{
    public interface IVirusCheck
    {
        public Task<bool> GetResultScan(Stream file, CancellationToken cancellationToken);
    }
}
