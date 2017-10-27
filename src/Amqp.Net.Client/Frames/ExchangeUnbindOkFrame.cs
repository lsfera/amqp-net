using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeUnbindOkFrame : MethodFrame<ExchangeUnbindOk>
    {
        internal ExchangeUnbindOkFrame(Int16 channelIndex, ExchangeUnbindOk payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}