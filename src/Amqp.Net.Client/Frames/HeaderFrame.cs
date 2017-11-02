using System;
using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal class HeaderFrame : Frame<HeaderFramePayload, RpcContext>
    {
        internal HeaderFrame(FrameHeader header, HeaderFramePayload payload)
            : base(header, payload)
        {
        }

        public override RpcContext Context => new RpcContext(this); // TODO: not useful

        public override Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel)
        {
            throw new NotImplementedException(); // TODO
        }

        public static IFrame Parse(FrameHeader header, IByteBuffer buffer)
        {
            var payload = HeaderFramePayload.Parse(buffer);

            return new HeaderFrame(header, payload);
        }
    }
}