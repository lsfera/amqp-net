using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Amqp.Net.Tests
{
    internal class TestOutputLogger : ILogger
    {
        private readonly ITestOutputHelper testOutputHelper;

        public TestOutputLogger(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, String> formatter)
        {
            testOutputHelper.WriteLine($"{logLevel} - {eventId} - {formatter(state, exception)}");
        }

        public Boolean IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}