using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ChannelOpenOkFrame : MethodFrame<ChannelOpenOk>
    {
        internal ChannelOpenOkFrame(Int16 channel, ChannelOpenOk payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }
    }
}