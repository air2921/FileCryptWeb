namespace application.Abstractions.Services.TP_Services
{
    public interface IGetSize
    {
        public double GetFileSizeInMb<T>(T file);
    }
}
