using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class DoubleFieldValueCodec : FieldValueCodec<Double>
    {
        internal static readonly FieldValueCodec<Double> Instance = new DoubleFieldValueCodec();

        public override Byte Type => 0x64;

        internal override Double Decode(IByteBuffer buffer)
        {
            return buffer.ReadDouble();
        }

        internal override void Encode(Double source, IByteBuffer buffer)
        {
            buffer.WriteDouble(source);
        }
    }
}