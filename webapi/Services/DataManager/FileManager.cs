using CsvHelper;
using Newtonsoft.Json.Linq;
using System.Globalization;
using webapi.Interfaces.Services;

namespace webapi.Services.DataManager
{
    public class FileManager : IFileManager
    {
        private readonly ILogger<FileManager> _logger;

        public FileManager(ILogger<FileManager> logger)
        {
            _logger = logger;
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
    }
}
