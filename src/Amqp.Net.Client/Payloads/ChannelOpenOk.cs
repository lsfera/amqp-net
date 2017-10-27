using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ChannelOpenOk : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(20, 11);

        internal readonly String Reserved1;

        internal static ChannelOpenOk Parse(IByteBuffer buffer)
        {
            return new ChannelOpenOk(LongStringFieldValueCodec.Instance.Decode(buffer));
        }

        internal ChannelOpenOk(String reserved1 = "")
        {
            Reserved1 = reserved1;
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            LongStringFieldValueCodec.Instance.Encode(Reserved1, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":\"{Reserved1}\"}}";
        }
    }
}