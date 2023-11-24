using CsvHelper;
using System.Globalization;
using webapi.Interfaces.Services;

namespace webapi.Services.DataManager
{
    public class FileManager : IFileManager
    {
        public HashSet<string> GetMimesFromCsvFile(string filePath)
        {
            var mimes = new HashSet<string>();

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        string mimeValue = csv.GetField<string>(1);
                        mimes.Add(mimeValue);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }

            return mimes;
        }
    }
}
