using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Amqp.Net.Tests
{
    internal class TestOtputLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper outputHelper;

        public TestOtputLoggerProvider(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(String categoryName)
        {
            return new TestOutputLogger(outputHelper);
        }
    }
}