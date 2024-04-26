namespace webapi.Helpers
{
    public class AdditionalLogger<T>(ILoggerFactory loggerFactory) : ICustomLogger<T>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger(typeof(T).FullName);

        public string RequestId { get; set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var token = RequestId ?? "None";
            var newState = $"RequestId: {token} \n{state}";

            _logger.Log(logLevel, eventId, newState, exception, formatter);
        }
    }

    public interface ICustomLogger<T> : ILogger<T>
    {
        public string RequestId { get; internal set; }
    }
}
