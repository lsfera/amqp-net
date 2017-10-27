using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeDeclareOkFrame : MethodFrame<ExchangeDeclareOk>
    {
        internal ExchangeDeclareOkFrame(Int16 channel, ExchangeDeclareOk payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }
    }
}