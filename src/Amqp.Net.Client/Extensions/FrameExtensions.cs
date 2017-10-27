using System;
using System.Threading.Tasks;
using Amqp.Net.Client.Frames;

namespace Amqp.Net.Client.Extensions
{
    internal static class FrameExtensions
    {
        internal static Task<T> SendAsync<T>(this T input, DotNetty.Transport.Channels.IChannel channel)
            where T : IFrame
        {
            return SendAsync(input, channel, _ => _);
        }

        internal static Task<TOutput> SendAsync<TInput, TOutput>(this TInput input,
                                                                 DotNetty.Transport.Channels.IChannel channel,
                                                                 Func<TInput, TOutput> func)
            where TInput : IFrame
            where TOutput : IFrame
        {
            var frame = func(input);

            return frame.WriteToAsync(channel)
                        .Then(() => frame);
        }

        internal static Task<T> ListenForAsync<T>(this Task<IFrame> task)
            where T : class, IFrame
        {
            return task.Then(_ =>
                             {
                                 if (!(_ is T result))
                                     throw new Exception($"cannot convert a frame of type '{_.GetType().Name}' to '{typeof(T).Name}'");
                             
                                 return result;
                             });
        }
    }
}