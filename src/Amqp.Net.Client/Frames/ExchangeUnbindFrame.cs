using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeUnbindFrame : MethodFrame<ExchangeUnbindPayload, RpcContext>
    {
        internal ExchangeUnbindFrame(Int16 channelIndex, ExchangeUnbindPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}