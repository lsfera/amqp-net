using System;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ExchangeUnbindOkPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(40, 51);

        internal static ExchangeUnbindOkPayload Parse(IByteBuffer buffer)
        {
            return new ExchangeUnbindOkPayload();
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