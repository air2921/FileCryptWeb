using Microsoft.Extensions.Logging;
using nClam;
using application.Abstractions.TP_Services;
using Microsoft.Extensions.Configuration;

namespace services.ClamAv
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public class ClamAV(ILogger<ClamAV> logger, IConfiguration configuration, IClamSetting clamSetting) : IVirusCheck
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
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
