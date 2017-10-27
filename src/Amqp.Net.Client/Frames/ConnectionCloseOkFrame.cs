using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionCloseOkFrame : MethodFrame<ConnectionCloseOk>
    {
        internal ConnectionCloseOkFrame(Int16 channel, ConnectionCloseOk payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }
    }
}