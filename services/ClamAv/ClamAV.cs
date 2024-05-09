using Microsoft.Extensions.Logging;
using nClam;
using application.Abstractions.TP_Services;
using Microsoft.Extensions.Configuration;

namespace services.ClamAv
{
    public class ClamAV(ILogger<ClamAV> logger, IConfiguration configuration, IClamSetting clamSetting) : IVirusCheck
    {
        public async Task<bool> GetResultScan(Stream fileStream, CancellationToken cancellationToken)
        {
            try
            {
                if (!int.TryParse(configuration["ClamPort"], out int port))
                {
                    logger.LogCritical("Clam Port cannot be converted");
                    return false;
                }

                var clam = clamSetting.SetClam(configuration["ClamServer"]!, port);

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

    public class ClamClientWrapper(ClamClient clamClient) : IClamClient
    {
        public async Task<ClamScanResult> SendAndScanFileAsync(Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                return await clamClient.SendAndScanFileAsync(stream, cancellationToken);
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
