using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicConsumeFrame : MethodFrame<BasicConsume>
    {
        internal BasicConsumeFrame(Int16 channelIndex, BasicConsume payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}