using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueBindFrame : MethodFrame<QueueBind>
    {
        internal QueueBindFrame(Int16 channelIndex, QueueBind payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}