using System;
using Amqp.Net.Client.Entities;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal struct FrameHeader
    {
        internal readonly FrameType Type;
        internal readonly Int16 ChannelIndex;

        internal FrameHeader(FrameType type, Int16 channelIndex)
        {
            Type = type;
            ChannelIndex = channelIndex;
        }

        public static FrameHeader Parse(IByteBuffer buffer)
        {
            return new FrameHeader((FrameType)buffer.ReadByte(),
                                   buffer.ReadShort());
        }

        public override Boolean Equals(Object obj)
        {
            if (!(obj is FrameHeader))
                return false;

            var header = (FrameHeader)obj;

            return Type == header.Type &&
                   ChannelIndex == header.ChannelIndex;
        }

        public override Int32 GetHashCode()
        {
            var hashCode = -1225776683;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + ChannelIndex.GetHashCode();

            return hashCode;
        }

        public override String ToString()
        {
            return $"{{\"frame_type\":\"{Type}\",\"channel_index\":{ChannelIndex}}}";
        }

        public static Boolean operator ==(FrameHeader a, FrameHeader b)
        {
            return a.Equals(b);
        }

        public static Boolean operator !=(FrameHeader a, FrameHeader b)
        {
            return !(a == b);
        }
    }
}