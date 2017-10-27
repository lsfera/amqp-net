using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class QueueDeleteOkFrame : MethodFrame<QueueDeleteOk>
    {
        internal QueueDeleteOkFrame(Int16 channelIndex, QueueDeleteOk payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }
    }
}