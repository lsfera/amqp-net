using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicQosOkFrame : MethodFrame<BasicQosOkPayload, RpcContext>
    {
        internal BasicQosOkFrame(Int16 channelIndex, BasicQosOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}