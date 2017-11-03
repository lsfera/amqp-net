using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    public class BodyFrame : Frame<BodyFramePayload, EmptyContext>
    {
        public BodyFrame(FrameHeader header, BodyFramePayload payload)
            : base(header, payload)
        {
        }
        
        public static IFrame Parse(FrameHeader header, IByteBuffer buffer)
        {
            return new BodyFrame(header, BodyFramePayload.Parse(buffer));
        }

        public override EmptyContext Context => new EmptyContext(this);

        public override Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel)
        {
            var buffer = channel.Allocator.Buffer();
            Payload.Write(buffer);

            return channel.WriteAndFlushAsync(buffer);
        }
    }
}