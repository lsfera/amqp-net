using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ChannelCloseFrame : MethodFrame<ChannelClosePayload, RpcContext>
    {
        internal static ChannelCloseFrame Close(Int16 channelIndex)
        {
            return new ChannelCloseFrame(channelIndex,
                                         new ChannelClosePayload(200,
                                                                 "connection_closed",
                                                                 20,
                                                                 41));
        }

        internal ChannelCloseFrame(Int16 channel, ChannelClosePayload payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}