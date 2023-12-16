namespace webapi.Interfaces.Services
{
    public interface IGetSize
    {
        public double GetFileSizeInMb<T>(T file);
    }
}
