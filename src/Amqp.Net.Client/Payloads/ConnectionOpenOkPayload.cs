using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ConnectionOpenOkPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(10, 41);

        internal readonly String Reserved1;

        internal ConnectionOpenOkPayload(String reserved1 = "")
        {
            Reserved1 = reserved1;
        }

        internal static ConnectionOpenOkPayload Parse(IByteBuffer buffer)
        {
            return new ConnectionOpenOkPayload(ShortStringFieldValueCodec.Instance.Decode(buffer));
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            ShortStringFieldValueCodec.Instance.Encode(Reserved1, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":\"{Reserved1}\"}}";
        }
    }
}