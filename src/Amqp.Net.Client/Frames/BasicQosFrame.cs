using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicQosFrame : MethodFrame<BasicQosPayload, RpcContext>
    {
        internal BasicQosFrame(Int16 channelIndex, BasicQosPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}