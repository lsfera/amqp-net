using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class Int64FieldValueCodec : FieldValueCodec<Int64>
    {
        internal static readonly FieldValueCodec<Int64> Instance = new Int64FieldValueCodec();

        public override Byte Type => 0x4C;

        internal override Int64 Decode(IByteBuffer buffer)
        {
            return buffer.ReadLong();
        }

        internal override void Encode(Int64 source, IByteBuffer buffer)
        {
            buffer.WriteLong(source);
        }
    }
}