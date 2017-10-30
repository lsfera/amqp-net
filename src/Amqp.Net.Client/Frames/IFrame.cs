using System;
using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal interface IFrame
    {
        FrameHeader Header { get; }

        IFramePayload Payload { get; }

        IFrameContext Context { get; }

        Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel);
    }

    internal interface IFrameContext
    {
        Int16 ChannelIndex { get; }
    }

    internal class RpcContext : IFrameContext
    {
        internal RpcContext(IFrame frame)
        {
            ChannelIndex = frame.Header.ChannelIndex;
        }

        public Int16 ChannelIndex { get; }
    }

    internal class AsyncContext : IFrameContext
    {
        internal AsyncContext(IFrame frame,
                              String consumerTag)
        {
            ChannelIndex = frame.Header.ChannelIndex;
            ConsumerTag = consumerTag;
        }

        public Int16 ChannelIndex { get; }

        internal String ConsumerTag { get; }
    }
}