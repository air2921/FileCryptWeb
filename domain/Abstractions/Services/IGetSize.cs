namespace domain.Abstractions.Services
{
    public interface IGetSize
    {
        public double GetFileSizeInMb<T>(T file);
    }
}
