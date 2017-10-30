using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicDeliverFrame : MethodFrame<BasicDeliver>
    {
        internal BasicDeliverFrame(Int16 channelIndex, BasicDeliver payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}