using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionTuneOkFrame : MethodFrame<ConnectionTuneOkPayload, RpcContext>
    {
        internal ConnectionTuneOkFrame(Int16 channel, ConnectionTuneOkPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}