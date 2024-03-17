using webapi.Interfaces.Services;
using webapi.Models;

namespace webapi.Helpers
{
    public class FileManager : IFileManager, IGetSize
    {
        private readonly ILogger<FileManager> _logger;

        public FileManager(ILogger<FileManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Use only in a test environment, if necessary replace with the specific uri of your frontend application
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isChrome"></param>
        /// <returns></returns>
        public string GetReactAppUrl()
        {
            return "https://localhost5173";
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
                _logger.LogCritical(ex.ToString());
                throw new ArgumentException("Invalid file");
            }
        }

        public void AddMimeCollection(ref HashSet<FileMimeModel> mimeModels, HashSet<string> existingMimes)
        {
            var mimeArray = new string[]
            {
                "video/ogg", "audio/ogg", "application/x-msdownload", "application/bat",
                "application/x-msdos-program", "application/javascript", "application/vnd.ms-word.document.macroEnabled.12",
                "application/vnd.ms-excel.sheet.macroEnabled.12", "application/pdf", "application/octet-stream",
                "application/x-httpd-php", "application/x-perl", "text/x-python", "application/x-sh", "application/x-powershell",
            };

            foreach (string mime in mimeArray)
                mimeModels.Add(new FileMimeModel { mime_name = mime });

            foreach (string existingMime in existingMimes)
                mimeModels.Add(new FileMimeModel { mime_name = existingMime });
        }
    }
}
