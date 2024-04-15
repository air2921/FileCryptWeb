namespace application.Services
{
    public class Response
    {
        public int Status { get; internal set; }
        public string? Message { get; internal set; } = null;
        public object? ObjectData { get; internal set; } = null;
    }
}
