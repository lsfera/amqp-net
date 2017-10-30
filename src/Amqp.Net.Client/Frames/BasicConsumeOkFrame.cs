using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicConsumeOkFrame : MethodFrame<BasicConsumeOk, RpcContext>
    {
        internal BasicConsumeOkFrame(Int16 channelIndex, BasicConsumeOk payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);

        public AsyncContext AsyncContext => new AsyncContext(this, Payload.ConsumerTag);
    }
}