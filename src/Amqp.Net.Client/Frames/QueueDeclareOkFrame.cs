using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueDeclareOkFrame : MethodFrame<QueueDeclareOk, RpcContext>
    {
        internal QueueDeclareOkFrame(Int16 channel, QueueDeclareOk payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}