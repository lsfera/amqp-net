using System;
using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace Amqp.Net.Client.Frames
{
    internal class MethodFrame<T> : Frame<T>
        where T : MethodFramePayload
    {
        internal MethodFrame(FrameHeader header, T payload)
            : base(header, payload)
        {
        }

        public override Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel)
        {
            var buffer = channel.Allocator.Buffer();
            buffer.WriteByte((Byte)Header.Type);
            buffer.WriteShort(Header.ChannelIndex);

            // TODO: not sure about being the best strategy; looks expansive...
            var b = Unpooled.Buffer();
            Payload.Descriptor.Write(b);
            Payload.Write(b);
            var array = b.ToArray();
            b.SafeRelease();

            buffer.WriteInt(array.Length);
            buffer.WriteBytes(array);
            buffer.WriteByte(0xCE);

            return channel.WriteAndFlushAsync(buffer);
        }
    }
}