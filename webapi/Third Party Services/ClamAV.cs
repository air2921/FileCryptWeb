using nClam;
using webapi.Helpers;
using webapi.Third_Party_Services.Abstractions;

namespace webapi.Third_Party_Services
{
    public class ClamAV(ILogger<ClamAV> logger, IConfiguration configuration, IClamSetting clamSetting) : IVirusCheck
    {
        public async Task<bool> GetResultScan(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                var fileStream = file.OpenReadStream();
                var clam = clamSetting.SetClam(configuration[App.CLAM_SERVER]!, int.Parse(configuration[App.CLAM_PORT]!));

                var scanResult = await clam.SendAndScanFileAsync(fileStream, cancellationToken);
                var rerult = scanResult.Result.Equals(ClamScanResults.Clean);

                return rerult;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
                return false;
            }
        }
    }

    public interface IClamClient
    {
        Task<ClamScanResult> SendAndScanFileAsync(Stream stream, CancellationToken cancellationToken);
    }

    public class ClamClientWrapper : IClamClient
    {
        private readonly ClamClient _clamClient;

        public ClamClientWrapper(ClamClient clamClient)
        {
            _clamClient = clamClient;
        }

        public async Task<ClamScanResult> SendAndScanFileAsync(Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                return await _clamClient.SendAndScanFileAsync(stream, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public interface IClamSetting
    {
        IClamClient SetClam(string server, int port);
    }

    public class ClamSetting : IClamSetting
    {
        public IClamClient SetClam(string server, int port)
        {
            return new ClamClientWrapper(new ClamClient(server, port)
            {
                MaxStreamSize = 75 * 1024 * 1024
            });
        }
    }
}
