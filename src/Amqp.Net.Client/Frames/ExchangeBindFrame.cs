using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeBindFrame : MethodFrame<ExchangeBindPayload, RpcContext>
    {
        internal ExchangeBindFrame(Int16 channelIndex, ExchangeBindPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}