using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicDeliverFrame : MethodFrame<BasicDeliverPayload, ConsumeContext>
    {
        internal BasicDeliverFrame(Int16 channelIndex,
                                   BasicDeliverPayload payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override ConsumeContext Context => new ConsumeContext(this, Payload.ConsumerTag);
    }
}