using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeDeclareFrame : MethodFrame<ExchangeDeclare>
    {
        internal ExchangeDeclareFrame(Int16 channelIndex, ExchangeDeclare payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}