using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class ByteFieldValueCodec : FieldValueCodec<Byte>
    {
        internal static readonly FieldValueCodec<Byte> Instance = new ByteFieldValueCodec();

        public override Byte Type => 0x42;

        internal override Byte Decode(IByteBuffer buffer)
        {
            return buffer.ReadByte();
        }

        internal override void Encode(Byte source, IByteBuffer buffer)
        {
            buffer.WriteByte(source);
        }
    }
}