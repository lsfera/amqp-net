using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeUnbindOkFrame : MethodFrame<ExchangeUnbindOkPayload, RpcContext>
    {
        internal ExchangeUnbindOkFrame(Int16 channelIndex, ExchangeUnbindOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}