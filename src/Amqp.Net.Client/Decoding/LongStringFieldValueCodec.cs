using System;
using System.Text;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class LongStringFieldValueCodec : FieldValueCodec<String>
    {
        private readonly Encoding encoding;

        internal static readonly FieldValueCodec<String> Instance = new LongStringFieldValueCodec(new UTF8Encoding(true));

        private LongStringFieldValueCodec(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Byte Type => 0x53;

        internal override String Decode(IByteBuffer buffer)
        {
            var length = buffer.ReadUnsignedInt();
            var destination = new Byte[length];
            buffer.ReadBytes(destination);

            return encoding.GetString(destination);
        }

        internal override void Encode(String source, IByteBuffer buffer)
        {
            var bytes = encoding.GetBytes(source);
            buffer.WriteUnsignedInt((UInt32)bytes.Length);
            buffer.WriteBytes(bytes);
        }
    }
}