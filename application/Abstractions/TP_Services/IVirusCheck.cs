namespace application.Abstractions.TP_Services
{
    public interface IVirusCheck
    {
        public Task<bool> GetResultScan(Stream file, CancellationToken cancellationToken);
    }
}
