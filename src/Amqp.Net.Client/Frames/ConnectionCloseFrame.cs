using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionCloseFrame : MethodFrame<ConnectionClosePayload, RpcContext>
    {
        internal static ConnectionCloseFrame Close()
        {
            return new ConnectionCloseFrame(0,
                                            new ConnectionClosePayload(200,
                                                                       "connection_closed",
                                                                       10,
                                                                       51));
        }

        internal ConnectionCloseFrame(Int16 channel, ConnectionClosePayload payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}