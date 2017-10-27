using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class Int16FieldValueCodec : FieldValueCodec<Int16>
    {
        internal static readonly FieldValueCodec<Int16> Instance = new Int16FieldValueCodec();

        public override Byte Type => 0x55;

        internal override Int16 Decode(IByteBuffer buffer)
        {
            return buffer.ReadShort();
        }

        internal override void Encode(Int16 source, IByteBuffer buffer)
        {
            buffer.WriteShort(source);
        }
    }
}