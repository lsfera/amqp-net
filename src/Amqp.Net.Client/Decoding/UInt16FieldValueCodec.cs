using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class UInt16FieldValueCodec : FieldValueCodec<UInt16>
    {
        internal static readonly FieldValueCodec<UInt16> Instance = new UInt16FieldValueCodec();

        public override Byte Type => 0x75;

        internal override UInt16 Decode(IByteBuffer buffer)
        {
            return buffer.ReadUnsignedShort();
        }

        internal override void Encode(UInt16 source, IByteBuffer buffer)
        {
            buffer.WriteUnsignedShort(source);
        }
    }
}