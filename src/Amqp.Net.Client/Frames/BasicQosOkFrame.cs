using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicQosOkFrame : MethodFrame<BasicQosOk>
    {
        internal BasicQosOkFrame(Int16 channelIndex, BasicQosOk payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}