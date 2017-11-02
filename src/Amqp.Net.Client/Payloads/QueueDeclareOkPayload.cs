using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class QueueDeclareOkPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(50, 11);

        internal readonly String Name;
        internal readonly Int32 MessageCount;
        internal readonly Int32 ConsumerCount;

        internal QueueDeclareOkPayload(String name,
                                       Int32 messageCount,
                                       Int32 consumerCount)
        {
            Name = name;
            MessageCount = messageCount;
            ConsumerCount = consumerCount;
        }

        internal static QueueDeclareOkPayload Parse(IByteBuffer buffer)
        {
            return new QueueDeclareOkPayload(ShortStringFieldValueCodec.Instance.Decode(buffer),
                                             Int32FieldValueCodec.Instance.Decode(buffer),
                                             Int32FieldValueCodec.Instance.Decode(buffer));
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            ShortStringFieldValueCodec.Instance.Encode(Name, buffer);
            Int32FieldValueCodec.Instance.Encode(MessageCount, buffer);
            Int32FieldValueCodec.Instance.Encode(ConsumerCount, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"name\":\"{Name}\",\"message_count\":{MessageCount},\"consumer_count\":{ConsumerCount}}}";
        }
    }
}