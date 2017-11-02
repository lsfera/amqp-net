using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicConsumeOkFrame : MethodFrame<BasicConsumeOkPayload, RpcContext>
    {
        internal BasicConsumeOkFrame(Int16 channelIndex, BasicConsumeOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);

        public ConsumeContext ConsumeContext => new ConsumeContext(this, Payload.ConsumerTag);
    }
}