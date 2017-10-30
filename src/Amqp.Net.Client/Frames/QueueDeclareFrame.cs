using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueDeclareFrame : MethodFrame<QueueDeclare, RpcContext>
    {
        internal QueueDeclareFrame(Int16 channelIndex, QueueDeclare payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}