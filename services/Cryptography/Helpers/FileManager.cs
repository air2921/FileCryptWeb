using Microsoft.Extensions.Logging;
using application.Abstractions.TP_Services;
using Microsoft.Extensions.Configuration;

namespace services.Cryptography.Helpers
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public class FileManager(ILogger<FileManager> logger, IConfiguration configuration) : IFileManager, IGetSize
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    {
        public string GetReactAppUrl()
        {
            return configuration["ReactDomain"]!;
        }

        public double GetFileSizeInMb<T>(T file)
        {
            try
            {
                switch (file)
                {
                    case Stream streamFile:
                        return (double)streamFile.Length / (1024 * 1024);
                    case long length:
                        return (double)length / (1024 * 1024);
                    case string filePath:
                        FileInfo fileInfo = new(filePath);
                        return (double)fileInfo.Length / (1024 * 1024);
                    default:
                        throw new ArgumentException("Unsupported file type");
                }
            }
            catch (IOException ex)
            {
                logger.LogCritical(ex.ToString());
                throw new ArgumentException("Invalid file");
            }
        }

        public HashSet<string> AddMimeCollection(HashSet<string> existingMimes)
        {
            var mimeArray = new string[]
            {
                "video/ogg", "audio/ogg", "application/x-msdownload", "application/bat",
                "application/x-msdos-program", "application/javascript", "application/vnd.ms-word.document.macroEnabled.12",
                "application/vnd.ms-excel.sheet.macroEnabled.12", "application/pdf", "application/octet-stream",
                "application/x-httpd-php", "application/x-perl", "text/x-python", "application/x-sh", "application/x-powershell",
            };

            existingMimes.UnionWith(mimeArray);
            return existingMimes;
        }
    }
}
