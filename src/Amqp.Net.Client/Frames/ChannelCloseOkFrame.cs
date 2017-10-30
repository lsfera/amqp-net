using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ChannelCloseOkFrame : MethodFrame<ChannelCloseOk, RpcContext>
    {
        internal ChannelCloseOkFrame(Int16 channel, ChannelCloseOk payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}