using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionStartFrame : MethodFrame<ConnectionStartPayload, RpcContext>
    {
        internal ConnectionStartFrame(Int16 channel, ConnectionStartPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}