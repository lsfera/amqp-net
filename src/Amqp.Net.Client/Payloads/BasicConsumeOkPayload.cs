using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class BasicConsumeOkPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(60, 21);

        internal readonly String ConsumerTag;
        internal BasicConsumeOkPayload(String consumerTag)
        {
            ConsumerTag = consumerTag;
        }

        internal static BasicConsumeOkPayload Parse(IByteBuffer buffer)
        {
            return new BasicConsumeOkPayload(ShortStringFieldValueCodec.Instance.Decode(buffer));
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            ShortStringFieldValueCodec.Instance.Encode(ConsumerTag, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"consumer_tag\":\"{ConsumerTag}\"}}";
        }
    }
}