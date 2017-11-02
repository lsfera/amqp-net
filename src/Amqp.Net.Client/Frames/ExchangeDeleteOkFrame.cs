using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeDeleteOkFrame : MethodFrame<ExchangeDeleteOkPayload, RpcContext>
    {
        internal ExchangeDeleteOkFrame(Int16 channel, ExchangeDeleteOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}