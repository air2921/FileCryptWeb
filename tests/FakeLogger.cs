using Microsoft.Extensions.Logging;

namespace tests
{
    internal class FakeLogger<T> : ILogger<T>
    {
        public FakeLogger()
        {
            LoggedMessages = new();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LoggedMessages.Add(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) => null;
        public List<string> LoggedMessages { get; }
    }
}
