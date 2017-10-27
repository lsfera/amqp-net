using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeDeleteFrame : MethodFrame<ExchangeDelete>
    {
        internal ExchangeDeleteFrame(Int16 channelIndex, ExchangeDelete payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}