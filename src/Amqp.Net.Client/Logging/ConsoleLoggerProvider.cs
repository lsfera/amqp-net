using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Amqp.Net.Client.Logging
{
    internal class ConsoleLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(String categoryName)
        {
            return new ConsoleLogger();
        }

        private class ConsoleLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel,
                                    EventId eventId,
                                    TState state,
                                    Exception exception,
                                    Func<TState, Exception, String> formatter)
            {
                Debug.WriteLine($"{logLevel} - {eventId} - {formatter(state, exception)}");
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
}