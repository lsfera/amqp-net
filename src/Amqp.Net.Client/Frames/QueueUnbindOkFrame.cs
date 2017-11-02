using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueUnbindOkFrame : MethodFrame<QueueUnbindOkPayload, RpcContext>
    {
        internal QueueUnbindOkFrame(Int16 channelIndex, QueueUnbindOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}