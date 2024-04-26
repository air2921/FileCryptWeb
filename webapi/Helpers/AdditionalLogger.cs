namespace webapi.Helpers
{
    //public class AdditionalLogger<T>(ILoggerFactory loggerFactory, IRequest request) : ILogger<T>
    //{
    //    private readonly ILogger _logger = loggerFactory.CreateLogger(typeof(T).FullName);

    //    public IDisposable BeginScope<TState>(TState state)
    //    {
    //        return _logger.BeginScope(state);
    //    }

    //    public bool IsEnabled(LogLevel logLevel)
    //    {
    //        return _logger.IsEnabled(logLevel);
    //    }

    //    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    //    {
    //        var token = request.Token ?? "None";
    //        var newState = $"RequestId: {token} \n{state}";

    //        _logger.Log(logLevel, eventId, newState, exception, formatter);
    //    }
    //}

    //public class Request : IRequest
    //{
    //    public string Token { get; set; }
    //}

    //public interface IRequest
    //{
    //    public string Token { get; set; }
    //}
}
