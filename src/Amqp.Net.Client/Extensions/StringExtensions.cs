using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Amqp.Net.Client.Extensions
{
    internal static class StringExtensions
    {
        internal static void Log(this String message, LogLevel level)
        {
            var entry = $"[{Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3, '0')}][{level.ToString().ToUpperInvariant().PadRight(7, ' ')}]:{message}";
            Debug.WriteLine(entry);
            WriteToConsole(level, entry);
        }

        private static void WriteToConsole(LogLevel level, String message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ColorMap[level];
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        private static readonly IDictionary<LogLevel, ConsoleColor> ColorMap = new Dictionary<LogLevel, ConsoleColor>
                                                                                   {
                                                                                       { LogLevel.Error, ConsoleColor.Red },
                                                                                       { LogLevel.Debug, ConsoleColor.Cyan },
                                                                                       { LogLevel.Information, ConsoleColor.White },
                                                                                       { LogLevel.Warning, ConsoleColor.Yellow },
                                                                                       { LogLevel.Trace, ConsoleColor.Blue },
                                                                                       { LogLevel.Critical, ConsoleColor.DarkRed }
                                                                                   };
    }
}