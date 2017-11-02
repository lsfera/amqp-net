using System;
using System.Collections.Generic;
using System.Linq;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    public class BasicDeliverFrame : MethodFrame<BasicDeliverPayload, ConsumeContext>
    {
        internal BasicDeliverFrame(Int16 channelIndex,
                                   BasicDeliverPayload payload,
                                   IEnumerable<IFrame> children)
            : base(new FrameHeader(FrameType.METHOD, channelIndex),
                                   payload,
                                   children)
        {
        }

        public override ConsumeContext Context => new ConsumeContext(this, Payload.ConsumerTag);

        public HeaderFrame ContentHeader => Children?.OfType<HeaderFrame>().FirstOrDefault();

        public override String ToString()
        {
            return $"{{\"clr_type\":\"{GetType().Name}\",\"header\":{Header},\"payload\":{Payload},\"content_header\":{ContentHeader}}}";
        }
    }
}