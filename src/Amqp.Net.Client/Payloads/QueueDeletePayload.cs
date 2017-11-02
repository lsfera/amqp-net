using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class QueueDeletePayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(50, 40);

        internal readonly Int16 Reserved1;
        internal readonly String Name;
        internal readonly Boolean IfUnused;
        internal readonly Boolean IfEmpty;
        internal readonly Boolean NoWait;

        internal QueueDeletePayload(Int16 reserved1,
                                    String name,
                                    Boolean ifUnused,
                                    Boolean ifEmpty,
                                    Boolean noWait)
        {
            Reserved1 = reserved1;
            Name = name;
            IfUnused = ifUnused;
            IfEmpty = ifEmpty;
            NoWait = noWait;
        }

        internal static QueueDeletePayload Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var name = ShortStringFieldValueCodec.Instance.Decode(buffer);

            var b = (Int32)buffer.ReadByte();
            var ifUnused = (b & 1) == 1;
            var ifEmpty = (b & 2) == 2;
            var noWait = (b & 4) == 4;

            return new QueueDeletePayload(reserved1,
                                          name,
                                          ifUnused,
                                          ifEmpty,
                                          noWait);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(Reserved1, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Name, buffer);

            var b = 0;

            if (IfUnused)
                b |= 1;

            if (IfEmpty)
                b |= 2;

            if (NoWait)
                b |= 4;

            buffer.WriteByte((Byte)b);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"name\":\"{Name}\",\"if_unused\":{IfUnused.ToString().ToLowerInvariant()},\"if_empty\":{IfEmpty.ToString().ToLowerInvariant()},\"no_wait\":{NoWait.ToString().ToLowerInvariant()}}}";
        }
    }
}