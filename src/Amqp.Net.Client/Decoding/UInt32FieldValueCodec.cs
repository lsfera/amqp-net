using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class UInt32FieldValueCodec : FieldValueCodec<UInt32>
    {
        internal static readonly FieldValueCodec<UInt32> Instance = new UInt32FieldValueCodec();

        public override Byte Type => 0x69;

        internal override UInt32 Decode(IByteBuffer buffer)
        {
            return buffer.ReadUnsignedInt();
        }

        internal override void Encode(UInt32 source, IByteBuffer buffer)
        {
            buffer.WriteUnsignedInt(source);
        }
    }
}