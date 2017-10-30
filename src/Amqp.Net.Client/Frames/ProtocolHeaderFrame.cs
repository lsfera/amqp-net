using System.Threading.Tasks;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal class ProtocolHeaderFrame : IFrame
    {
        internal static readonly ProtocolHeaderFrame Instance = new ProtocolHeaderFrame();

        public FrameHeader Header => new FrameHeader(FrameType.METHOD, 0);

        public IFramePayload Payload => new ProtocolHeader();

        public Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel)
        {
            var buffer = Unpooled.Buffer();
            Payload.Write(buffer);

            return channel.WriteAndFlushAsync(buffer);
        }
    }
}