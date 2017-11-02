using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class BasicDeliverPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(60, 60);

        internal readonly String ConsumerTag;
        internal readonly Int64 DeliveryTag;
        internal readonly Boolean Redelivered;
        internal readonly String Exchange;
        internal readonly String RoutingKey;

        internal BasicDeliverPayload(String consumerTag,
                                     Int64 deliveryTag,
                                     Boolean redelivered,
                                     String exchange,
                                     String routingKey)
        {
            ConsumerTag = consumerTag;
            DeliveryTag = deliveryTag;
            Redelivered = redelivered;
            Exchange = exchange;
            RoutingKey = routingKey;
        }

        internal static BasicDeliverPayload Parse(IByteBuffer buffer)
        {
            var consumerTag = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var deliveryTag = Int64FieldValueCodec.Instance.Decode(buffer);

            var b = (Int32)buffer.ReadByte();
            var redelivered = (b & 1) == 1;

            var exchange = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var routingKey = ShortStringFieldValueCodec.Instance.Decode(buffer);

            return new BasicDeliverPayload(consumerTag,
                                           deliveryTag,
                                           redelivered,
                                           exchange,
                                           routingKey);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            ShortStringFieldValueCodec.Instance.Encode(ConsumerTag, buffer);
            Int64FieldValueCodec.Instance.Encode(DeliveryTag, buffer);

            var b = 0;

            if (Redelivered)
                b |= 1;

            buffer.WriteByte((Byte)b);

            ShortStringFieldValueCodec.Instance.Encode(Exchange, buffer);
            ShortStringFieldValueCodec.Instance.Encode(RoutingKey, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"consumer_tag\":\"{ConsumerTag}\",\"delivery_tag\":{DeliveryTag},\"redelivered\":{Redelivered.ToString().ToLowerInvariant()},\"exchange\":\"{Exchange}\",\"routing_key\":\"{RoutingKey}\"}}";
        }
    }
}