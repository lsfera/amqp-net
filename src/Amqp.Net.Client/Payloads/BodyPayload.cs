using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    public class BodyPayload : IFramePayload
    {
        internal readonly Byte[] Content;

        public BodyPayload(Byte[] content)
        {
            Content = content;
        }

        public void Write(IByteBuffer buffer)
        {
            buffer.WriteBytes(Content);
        }

        public static BodyPayload Parse(IByteBuffer buffer)
        {
            if (buffer.ReadableBytes <= 0)
                return new BodyPayload(new Byte[] { });

            var content = new Byte[buffer.ReadableBytes];
            buffer.ReadBytes(content);

            return new BodyPayload(content);
        }
    }
}