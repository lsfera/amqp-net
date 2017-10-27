using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class SingleFieldValueCodec : FieldValueCodec<Single>
    {
        internal static readonly FieldValueCodec<Single> Instance = new SingleFieldValueCodec();

        public override Byte Type => 0x66;

        internal override Single Decode(IByteBuffer buffer)
        {
            return buffer.ReadFloat();
        }

        internal override void Encode(Single source, IByteBuffer buffer)
        {
            buffer.WriteFloat(source);
        }
    }
}