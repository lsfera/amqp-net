using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class BooleanFieldValueCodec : FieldValueCodec<Boolean>
    {
        internal static readonly FieldValueCodec<Boolean> Instance = new BooleanFieldValueCodec();

        public override Byte Type => 0x74;

        internal override Boolean Decode(IByteBuffer buffer)
        {
            return buffer.ReadByte() != 0;
        }

        internal override void Encode(Boolean source, IByteBuffer buffer)
        {
            buffer.WriteByte(source ? 1 : 0);
        }
    }
}