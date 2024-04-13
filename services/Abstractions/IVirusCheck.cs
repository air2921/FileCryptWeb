namespace services.Abstractions
{
    public interface IVirusCheck
    {
        public Task<bool> GetResultScan(Stream file, CancellationToken cancellationToken);
    }
}
