using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ConnectionTunePayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(10, 30);

        internal readonly Int16 ChannelMax;
        internal readonly Int32 FrameMax;
        internal readonly Int16 Heartbeat;

        internal static ConnectionTunePayload Parse(IByteBuffer buffer)
        {
            return new ConnectionTunePayload(Int16FieldValueCodec.Instance.Decode(buffer),
                                             Int32FieldValueCodec.Instance.Decode(buffer),
                                             Int16FieldValueCodec.Instance.Decode(buffer));
        }

        internal ConnectionTunePayload(Int16 channelMax,
                                       Int32 frameMax,
                                       Int16 heartbeat)
        {
            ChannelMax = channelMax;
            FrameMax = frameMax;
            Heartbeat = heartbeat;
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(ChannelMax, buffer);
            Int32FieldValueCodec.Instance.Encode(FrameMax, buffer);
            Int16FieldValueCodec.Instance.Encode(Heartbeat, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"channel_max\":{ChannelMax},\"frame_max\":{FrameMax},\"heartbeat\":{Heartbeat}}}";
        }
    }
}