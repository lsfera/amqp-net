using System;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal abstract class MethodFramePayload : IFramePayload
    {
        internal abstract MethodFrameDescriptor Descriptor { get; }

        public void Write(IByteBuffer buffer)
        {
            WriteInternal(buffer);
        }

        protected abstract void WriteInternal(IByteBuffer buffer);

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor}}}";
        }
    }
}