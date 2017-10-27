using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ExchangeDelete : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(40, 20);

        internal readonly Int16 Reserved1;
        internal readonly String Name;
        internal readonly Boolean IfUnused;
        internal readonly Boolean NoWait;

        internal ExchangeDelete(Int16 reserved1,
                                String name,
                                Boolean ifUnused,
                                Boolean noWait)
        {
            Reserved1 = reserved1;
            Name = name;
            IfUnused = ifUnused;
            NoWait = noWait;
        }

        internal static ExchangeDelete Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var name = ShortStringFieldValueCodec.Instance.Decode(buffer);

            var b = (Int32)buffer.ReadByte();
            var ifUnused = (b & 1) == 1;
            var noWait = (b & 2) == 2;

            return new ExchangeDelete(reserved1,
                                      name,
                                      ifUnused,
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

            if (NoWait)
                b |= 2;

            buffer.WriteByte((Byte)b);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"name\":\"{Name}\",\"if_unused\":{IfUnused.ToString().ToLowerInvariant()},\"no_wait\":{NoWait.ToString().ToLowerInvariant()}}}";
        }
    }
}