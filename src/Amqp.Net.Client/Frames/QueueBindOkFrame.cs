using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueBindOkFrame : MethodFrame<QueueBindOk>
    {
        internal QueueBindOkFrame(Int16 channelIndex, QueueBindOk payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}