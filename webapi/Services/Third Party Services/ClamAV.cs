using nClam;
using webapi.Interfaces.Services;

namespace webapi.Services.Third_Party_Services
{
    public class ClamAV : IVirusCheck
    {
        private readonly ILogger<ClamAV> _logger;
        public ClamAV(ILogger<ClamAV> logger)
        {
            _logger = logger;
        }

        public async Task<bool> GetResultScan(IFormFile file)
        {
            try
            {
                var fileStream = file.OpenReadStream();
                var clam = new ClamClient("localhost", 3310);
                clam.MaxStreamSize = 75 * 1024 * 1024;

                var scanResult = await clam.SendAndScanFileAsync(fileStream);
                return scanResult.Result == ClamScanResults.Clean;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());

                return false;
            }
        }
    }
}
