namespace webapi.Interfaces.Services
{
    public interface IGetSize
    {
        public double GetFolderSizeInMb(string folderPath);
        public double GetFileSizeInMb<T>(T file);
    }
}
