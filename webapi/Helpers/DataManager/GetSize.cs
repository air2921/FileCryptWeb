using webapi.Interfaces.Services;

namespace webapi.Helpers.DataManager
{
    public class GetSize : IGetSize
    {
        private long GetFolderSize(string FolderPath)
        {
            long totalSize = 0;

            foreach (var file in Directory.GetFiles(FolderPath))
            {
                totalSize += new FileInfo(file).Length;
            }

            foreach (var subDirectory in Directory.GetDirectories(FolderPath))
            {
                totalSize += GetFolderSize(subDirectory);
            }

            return totalSize;
        }

        public double GetFileSizeInMb<T>(T file)
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
    }
}
