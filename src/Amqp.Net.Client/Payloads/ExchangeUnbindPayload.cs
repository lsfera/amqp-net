using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Utils;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ExchangeUnbindPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(40, 40);

        internal readonly Int16 Reserved1;
        internal readonly String Destination;
        internal readonly String Source;
        internal readonly String RoutingKey;
        internal readonly Boolean NoWait;
        internal readonly Table Arguments;

        internal ExchangeUnbindPayload(Int16 reserved1,
                                       String destination,
                                       String source,
                                       String routingKey,
                                       Boolean noWait,
                                       Table arguments) // TODO: arguments should be a bit less "generic"
        {
            Reserved1 = reserved1;

            ValidationUtils.ValidateExchangeName(destination);
            Destination = destination;

            ValidationUtils.ValidateExchangeName(source);
            Source = source;

            RoutingKey = routingKey;
            NoWait = noWait;
            Arguments = arguments;
        }

        internal static ExchangeUnbindPayload Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var queueName = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var exchangeName = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var routingKey = ShortStringFieldValueCodec.Instance.Decode(buffer);

            var b = buffer.ReadByte();
            var noWait = (b & 1) == 1;

            var arguments = TableFieldValueCodec.Instance.Decode(buffer);

            return new ExchangeUnbindPayload(reserved1,
                                             queueName,
                                             exchangeName,
                                             routingKey,
                                             noWait,
                                             arguments);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(Reserved1, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Destination, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Source, buffer);
            ShortStringFieldValueCodec.Instance.Encode(RoutingKey, buffer);

            var b = 0;

            if (NoWait)
                b |= 1;

            buffer.WriteByte((Byte)b);

            TableFieldValueCodec.Instance.Encode(Arguments, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"destination\":\"{Destination}\",\"source\":\"{Source}\",\"routing_key\":\"{RoutingKey}\",\"no_wait\":\"{NoWait}\",\"arguments\":{Arguments}}}";
        }
    }
}