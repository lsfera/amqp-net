using System;
using System.Text;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class LongStringFieldValueCodec : FieldValueCodec<String>
    {
        internal static readonly FieldValueCodec<String> Instance = new LongStringFieldValueCodec();

        public override Byte Type => 0x53;

        internal override String Decode(IByteBuffer buffer)
        {
            var length = buffer.ReadUnsignedInt();
            var destination = new Byte[length];
            buffer.ReadBytes(destination);

            return new UTF8Encoding(true).GetString(destination);
        }

        internal override void Encode(String source, IByteBuffer buffer)
        {
            var bytes = new UTF8Encoding(true).GetBytes(source);
            buffer.WriteUnsignedInt((UInt32)bytes.Length);
            buffer.WriteBytes(bytes);
        }
    }
}