using System;
using System.Text;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class ShortStringFieldValueCodec : FieldValueCodec<String>
    {
        internal static readonly FieldValueCodec<String> Instance = new ShortStringFieldValueCodec();

        public override Byte Type => 0x73;

        internal override String Decode(IByteBuffer buffer)
        {
            var length = buffer.ReadByte();
            var destination = new Byte[length];
            buffer.ReadBytes(destination);

            // TODO: we need just one instance per-thread
            return new UTF8Encoding(true).GetString(destination);
        }

        internal override void Encode(String source, IByteBuffer buffer)
        {
            var bytes = new UTF8Encoding(true).GetBytes(source);
            buffer.WriteByte(bytes.Length);
            buffer.WriteBytes(bytes);
        }
    }
}