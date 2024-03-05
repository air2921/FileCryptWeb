using Microsoft.AspNetCore.Http;
using webapi.Middlewares;

namespace tests.Middlewares_Tests
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
}
