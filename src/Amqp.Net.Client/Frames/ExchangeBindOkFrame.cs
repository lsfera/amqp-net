using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ExchangeBindOkFrame : MethodFrame<ExchangeBindOk, RpcContext>
    {
        internal ExchangeBindOkFrame(Int16 channelIndex, ExchangeBindOk payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}