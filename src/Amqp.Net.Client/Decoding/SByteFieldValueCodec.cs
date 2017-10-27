using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    // TODO: remove?
    internal class SByteFieldValueCodec : FieldValueCodec<SByte>
    {
        internal static readonly FieldValueCodec<SByte> Instance = new SByteFieldValueCodec();

        public override Byte Type => 0x62;

        internal override SByte Decode(IByteBuffer buffer)
        {
            return Convert.ToSByte(buffer.ReadByte());
        }

        internal override void Encode(SByte source, IByteBuffer buffer)
        {
            buffer.WriteByte(source);
        }
    }
}