using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ChannelCloseFrame : MethodFrame<ChannelClose, RpcContext>
    {
        internal static ChannelCloseFrame Close(Int16 channelIndex)
        {
            return new ChannelCloseFrame(channelIndex,
                                         new ChannelClose(200,
                                                          "connection_closed",
                                                          20,
                                                          41));
        }

        internal ChannelCloseFrame(Int16 channel, ChannelClose payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}