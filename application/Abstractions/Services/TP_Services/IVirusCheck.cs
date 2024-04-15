namespace application.Abstractions.Services.TP_Services
{
    public interface IVirusCheck
    {
        public Task<bool> GetResultScan(Stream file, CancellationToken cancellationToken);
    }
}
