using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueBindOkFrame : MethodFrame<QueueBindOkPayload, RpcContext>
    {
        internal QueueBindOkFrame(Int16 channelIndex, QueueBindOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}