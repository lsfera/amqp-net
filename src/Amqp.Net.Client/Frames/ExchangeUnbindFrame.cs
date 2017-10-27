using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeUnbindFrame : MethodFrame<ExchangeUnbind>
    {
        internal ExchangeUnbindFrame(Int16 channelIndex, ExchangeUnbind payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}