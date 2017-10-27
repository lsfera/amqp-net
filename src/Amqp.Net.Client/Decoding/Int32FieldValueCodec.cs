using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class Int32FieldValueCodec : FieldValueCodec<Int32>
    {
        internal static readonly FieldValueCodec<Int32> Instance = new Int32FieldValueCodec();

        public override Byte Type => 0x49;

        internal override Int32 Decode(IByteBuffer buffer)
        {
            return buffer.ReadInt();
        }

        internal override void Encode(Int32 source, IByteBuffer buffer)
        {
            buffer.WriteInt(source);
        }
    }
}