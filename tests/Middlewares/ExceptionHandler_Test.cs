using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using webapi.Middlewares;

namespace tests.Middlewares
{
    public class ExceptionHandler_Test
    {
        [Fact]
        public async Task Invoke_Exception_LogsError_Returns500StatusCode()
        {
            var context = new DefaultHttpContext();
            var logger = new FakeLogger<ExceptionHandleMiddleware>();
            var middleware = new ExceptionHandleMiddleware((innerHttpContext) => throw new Exception("Test exception"), logger);

            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            await middleware.Invoke(context);

            responseStream.Position = 0;
            var reader = new StreamReader(responseStream);
            var responseBody = await reader.ReadToEndAsync();

            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
            Assert.Equal("{\"message\":\"Unexpected error. Don't worry, we already working on it\"}", responseBody);
            Assert.Single(logger.LoggedMessages);
            Assert.Contains("Test exception", logger.LoggedMessages[0]);
        }
    }

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
