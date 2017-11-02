using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeDeclareOkFrame : MethodFrame<ExchangeDeclareOkPayload, RpcContext>
    {
        internal ExchangeDeclareOkFrame(Int16 channel, ExchangeDeclareOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}