using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal class BodyFrame : Frame<BodyFramePayload, RpcContext>
    {
        public BodyFrame(FrameHeader header, BodyFramePayload payload)
            : base(header, payload)
        {
        }
        
        public static IFrame Parse(FrameHeader header, IByteBuffer buffer)
        {
            var payload = BodyFramePayload.Parse(buffer);

            return new BodyFrame(header, payload);
        }

        public override RpcContext Context => new RpcContext(this); // TODO: not useful

        public override Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel)
        {
            var buffer = channel.Allocator.Buffer();
            Payload.Write(buffer);

            return channel.WriteAndFlushAsync(buffer);
        }
    }
}