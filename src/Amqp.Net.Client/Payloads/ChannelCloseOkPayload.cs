using System;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ChannelCloseOkPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(20, 41);

        internal static ChannelCloseOkPayload Parse(IByteBuffer buffer)
        {
            return new ChannelCloseOkPayload();
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