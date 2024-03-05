using CsvHelper;
using Newtonsoft.Json.Linq;
using System.Globalization;
using webapi.Interfaces.Services;

namespace webapi.Helpers
{
    public class FileManager : IFileManager, IGetSize
    {
        private readonly ILogger<FileManager> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileManager(ILogger<FileManager> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public HashSet<string> GetMimesFromCsvFile(string filePath)
        {
            var mimes = new HashSet<string>();

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    string mimeValue = csv.GetField<string>(1);
                    if (!string.IsNullOrWhiteSpace(mimeValue))
                        mimes.Add(mimeValue);
                    else
                        continue;
                }
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(GetMimesFromCsvFile));
            }
            catch (CsvHelperException ex)
            {
                _logger.LogCritical(ex.ToString(), nameof(GetMimesFromCsvFile));
            }

            return mimes;
        }

        public string[] GetCsvFiles()
        {
            var basePath = Path.Combine(_webHostEnvironment.ContentRootPath, "..", "data");
            return Directory.GetFiles(basePath);
        }

        /// <summary>
        /// Use only in a test environment, if necessary replace with the specific uri of your frontend application
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isChrome"></param>
        /// <returns></returns>
        public string GetReactAppUrl(string path, bool isChrome)
        {
            string jsonContent = File.ReadAllText(path);

            JObject launchJson = JObject.Parse(jsonContent);

            string edgeUrl = launchJson["configurations"][0]["url"].ToString();

            string chromeUrl = launchJson["configurations"][1]["url"].ToString();

            if (isChrome)
                return chromeUrl;

            return edgeUrl;
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
    }
}
