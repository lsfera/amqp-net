using System;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class BasicQosOkPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(60, 11);

        internal static BasicQosOkPayload Parse(IByteBuffer buffer)
        {
            return new BasicQosOkPayload();
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor}}}";
        }
    }
}