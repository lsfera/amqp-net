using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeBindFrame : MethodFrame<ExchangeBind>
    {
        internal ExchangeBindFrame(Int16 channelIndex, ExchangeBind payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}