using System;
using System.Diagnostics;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Payloads;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace Amqp.Net.Client.Handlers
{
    internal class MessageHandler : ChannelHandlerAdapter
    {
        private readonly IMethodFrameBag bag;
        private readonly IFrameParser parser;

        public MessageHandler(IMethodFrameBag bag, IFrameParser parser)
        {
            this.bag = bag;
            this.parser = parser;
        }

        public override void ChannelRead(IChannelHandlerContext context, Object message)
        {
            if (!(message is IByteBuffer buffer))
                return;

            var result = parser.Parse(buffer);

            if (result.Payload is MethodFramePayload payload)
                bag.For(payload.Descriptor)
                   .Pop(result.Header.ChannelIndex)
                   .SetResult(result);
            else
                throw new NotSupportedException("implementation is ongoing...");

            buffer.SafeRelease();
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            // TODO: should be managed at connection level.
            Debug.WriteLine("exception: " + exception);
            context.CloseAsync();
        }
    }
}