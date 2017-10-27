using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicQosFrame : MethodFrame<BasicQos>
    {
        internal BasicQosFrame(Int16 channelIndex, BasicQos payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}