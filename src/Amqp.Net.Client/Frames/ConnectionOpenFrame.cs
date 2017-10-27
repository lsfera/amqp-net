using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionOpenFrame : MethodFrame<ConnectionOpen>
    {
        internal ConnectionOpenFrame(Int16 channel, ConnectionOpen payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }
    }
}