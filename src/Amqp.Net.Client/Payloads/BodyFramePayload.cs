using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class BodyFramePayload : IFramePayload
    {
        internal readonly Byte[] Content;

        public BodyFramePayload(Byte[] content)
        {
            Content = content;
        }

        public void Write(IByteBuffer buffer)
        {
            buffer.WriteBytes(Content);
        }

        public static BodyFramePayload Parse(IByteBuffer buffer)
        {
            var content = new Byte[buffer.ReadableBytes];
            buffer.ReadBytes(content);

            return new BodyFramePayload(content);
        }
    }
}