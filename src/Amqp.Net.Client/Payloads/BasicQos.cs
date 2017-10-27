using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class BasicQos : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(60, 10);

        /// <remarks>
        /// This field specifies the prefetch window size in octets.
        /// </remarks>
        internal readonly Int32 PrefetchSize;

        /// <remarks>
        /// Specifies a prefetch window in terms of whole messages.
        /// </remarks>
        internal readonly Int16 PrefetchCount;
        internal readonly Boolean Global;

        internal BasicQos(Int32 prefetchSize,
                          Int16 prefetchCount,
                          Boolean global)
        {
            PrefetchSize = prefetchSize;
            PrefetchCount = prefetchCount;
            Global = global;
        }

        internal static BasicQos Parse(IByteBuffer buffer)
        {
            var prefetchSize = Int32FieldValueCodec.Instance.Decode(buffer);
            var prefetchCount = Int16FieldValueCodec.Instance.Decode(buffer);
            var b = (Int32)buffer.ReadByte();
            var global = (b & 1) == 1;

            return new BasicQos(prefetchSize,
                                prefetchCount,
                                global);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int32FieldValueCodec.Instance.Encode(PrefetchSize, buffer);
            Int16FieldValueCodec.Instance.Encode(PrefetchCount, buffer);

            var b = 0;

            if (Global)
                b |= 1;

            buffer.WriteByte((Byte)b);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"prefetch_size\":{PrefetchSize},\"prefetch_count\":{PrefetchCount},\"global\":{Global}}}";
        }
    }
}