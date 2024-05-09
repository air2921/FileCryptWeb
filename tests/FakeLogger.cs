using Microsoft.Extensions.Logging;

namespace tests
{
    internal class FakeLogger<T> : ILogger<T>
    {
        public FakeLogger()
        {
            LoggedMessages = [];
        }

#pragma warning disable CS8767 // Допустимость значений NULL для ссылочных типов в типе параметра не соответствует неявно реализованному элементу (возможно, из-за атрибутов допустимости значений NULL).
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
#pragma warning restore CS8767 // Допустимость значений NULL для ссылочных типов в типе параметра не соответствует неявно реализованному элементу (возможно, из-за атрибутов допустимости значений NULL).
        {
            LoggedMessages.Add(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel) => true;
#pragma warning disable CS8633 // Допустимость значения NULL в ограничениях для параметра типа не соответствует ограничениям параметра типа в явно реализованном методе интерфейса.
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        public IDisposable BeginScope<TState>(TState state) => null;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
#pragma warning restore CS8633 // Допустимость значения NULL в ограничениях для параметра типа не соответствует ограничениям параметра типа в явно реализованном методе интерфейса.
        public List<string> LoggedMessages { get; }
    }
}
