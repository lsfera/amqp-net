using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeBindOkFrame : MethodFrame<ExchangeBindOkPayload, RpcContext>
    {
        internal ExchangeBindOkFrame(Int16 channelIndex, ExchangeBindOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}