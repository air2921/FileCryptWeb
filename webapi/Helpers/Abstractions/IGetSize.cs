namespace webapi.Helpers.Abstractions
{
    public interface IGetSize
    {
        public double GetFileSizeInMb<T>(T file);
    }
}
