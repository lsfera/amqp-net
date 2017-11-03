using System;
using System.Threading.Tasks;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace Amqp.Net.Client.Frames
{
    public class HeaderFrame : Frame<HeaderFramePayload, EmptyContext>
    {
        internal HeaderFrame(Int16 channelIndex, HeaderFramePayload payload)
            : base(new FrameHeader(FrameType.HEADER, channelIndex), payload)
        {
        }

        public override EmptyContext Context => new EmptyContext(this);

        public override Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel)
        {
            var buffer = channel.Allocator.Buffer();
            buffer.WriteByte((Byte)Header.Type);
            buffer.WriteShort(Header.ChannelIndex);

            // TODO: not sure about being the best strategy; looks expansive...
            var b = Unpooled.Buffer();
            Payload.Write(b);
            var array = b.ToArray();
            b.SafeRelease();

            buffer.WriteInt(array.Length);
            buffer.WriteBytes(array);
            buffer.WriteByte(0xCE);

            return channel.WriteAndFlushAsync(buffer);
        }

        public static IFrame Parse(FrameHeader header, IByteBuffer buffer)
        {
            return new HeaderFrame(header.ChannelIndex, HeaderFramePayload.Parse(buffer));
        }
    }
}