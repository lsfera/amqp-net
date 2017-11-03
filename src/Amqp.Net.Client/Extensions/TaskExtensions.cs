using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Amqp.Net.Client.Extensions
{
    internal static class TaskExtensions
    {
        internal static Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task,
                                                                   Func<TResult, TNewResult> func)
        {
            if (task.Exception != null)
                throw task.Exception.GetBaseException();

            return task.ContinueWith(_ => func(_.Result));
        }

        internal static Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task,
                                                                   Func<TResult, Task<TNewResult>> func)
        {
            if (task.Exception != null)
                throw task.Exception.GetBaseException();

            return task.ContinueWith(_ => func(_.Result))
                       .Unwrap();
        }

        internal static Task<TNewResult> Then<TNewResult>(this Task task,
                                                          Func<TNewResult> func)
        {
            if (task.Exception != null)
                throw task.Exception.GetBaseException();

            return task.ContinueWith(_ => func());
        }

        internal static Task<TResult> LogError<TResult>(this Task<TResult> task)
        {
            task.Exception?.GetBaseException().Log();

            return task;
        }

        internal static Task LogError(this Task task)
        {
            task.Exception?.GetBaseException().Log();

            return task;
        }

        internal static void Log(this Exception exception)
        {
            $"{exception.Message}\n{exception.StackTrace}".Log(LogLevel.Error);
        }

        internal static Task<TResult> Log<TResult>(this Task<TResult> task,
                                                   Func<TResult, String> func,
                                                   LogLevel level = LogLevel.Debug)
        {
            if (task.Exception == null)
                func(task.Result).Log(level);

            return task;
        }
    }
}