using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal abstract class Frame<TPayload, TContext> : IFrame
        where TPayload : class, IFramePayload
        where TContext : IFrameContext
    {
        internal readonly IEnumerable<IFrame> Children;
        public readonly TPayload Payload;

        internal Frame(FrameHeader header, TPayload payload, IEnumerable<IFrame> children = null)
        {
            Header = header;
            Payload = payload;
            Children = children ?? new List<IFrame>();
        }

        public FrameHeader Header { get; }

        IFramePayload IFrame.Payload => Payload;

        public abstract TContext Context { get; }

        IFrameContext IFrame.Context => Context;

        public abstract Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel);

        public override String ToString()
        {
            return $"{{\"clr_type\":\"{GetType().Name}\",\"header\":{Header},\"payload\":{Payload}}}";
        }
    }
}