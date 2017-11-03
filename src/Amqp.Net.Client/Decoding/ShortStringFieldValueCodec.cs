using System;
using System.Text;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class ShortStringFieldValueCodec : FieldValueCodec<String>
    {
        private readonly Encoding encoding;

        internal static readonly FieldValueCodec<String> Instance = new ShortStringFieldValueCodec(new UTF8Encoding(true));

        private ShortStringFieldValueCodec(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Byte Type => 0x73;

        internal override String Decode(IByteBuffer buffer)
        {
            var length = buffer.ReadByte();
            var destination = new Byte[length];
            buffer.ReadBytes(destination);

            return encoding.GetString(destination);
        }

        internal override void Encode(String source, IByteBuffer buffer)
        {
            var bytes = encoding.GetBytes(source);
            buffer.WriteByte(bytes.Length);
            buffer.WriteBytes(bytes);
        }
    }
}