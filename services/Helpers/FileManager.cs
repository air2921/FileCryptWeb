using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using application.Abstractions.Services.TP_Services;

namespace services.Helpers
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public class FileManager(ILogger<FileManager> logger) : IFileManager, IGetSize
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    {
        public string ReactConnection { private get; set; }

        public string GetReactAppUrl()
        {
            return ReactConnection!;
        }

        public double GetFileSizeInMb<T>(T file)
        {
            try
            {
                switch (file)
                {
                    case IFormFile formFile:
                        return (double)formFile.Length / (1024 * 1024);
                    case string filePath:
                        FileInfo fileInfo = new FileInfo(filePath);
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

        public IEnumerable<string> AddMimeCollection(HashSet<string> existingMimes)
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
