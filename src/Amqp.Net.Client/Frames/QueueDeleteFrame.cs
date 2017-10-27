using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueDeleteFrame : MethodFrame<QueueDelete>
    {
        internal QueueDeleteFrame(Int16 channelIndex, QueueDelete payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}