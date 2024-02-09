using nClam;
using webapi.Interfaces.Services;

namespace webapi.Services.Third_Party_Services
{
    public class ClamAV : IVirusCheck
    {
        private readonly ILogger<ClamAV> _logger;
        private readonly IClamSetting _clamSetting;

        public ClamAV(ILogger<ClamAV> logger, IClamSetting clamSetting)
        {
            _logger = logger;
            _clamSetting = clamSetting;
        }

        public async Task<bool> GetResultScan(IFormFile file)
        {
            try
            {
                var fileStream = file.OpenReadStream();
                var clam = _clamSetting.SetClam();

                var scanResult = await clam.SendAndScanFileAsync(fileStream);
                var rerult = scanResult.Result.Equals(ClamScanResults.Clean);

                return rerult;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());

                return false;
            }
        }
    }

    public interface IClamClient
    {
        Task<ClamScanResult> SendAndScanFileAsync(Stream stream);
    }

    public class ClamClientWrapper : IClamClient
    {
        private readonly ClamClient _clamClient;

        public ClamClientWrapper(ClamClient clamClient)
        {
            _clamClient = clamClient;
        }

        public async Task<ClamScanResult> SendAndScanFileAsync(Stream stream)
        {
            try
            {
                return await _clamClient.SendAndScanFileAsync(stream);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public interface IClamSetting
    {
        IClamClient SetClam();
    }

    public class ClamSetting : IClamSetting
    {
        public IClamClient SetClam()
        {
            return new ClamClientWrapper(new ClamClient("localhost", 3310)
            {
                MaxStreamSize = 75 * 1024 * 1024
            });
        }
    }
}
