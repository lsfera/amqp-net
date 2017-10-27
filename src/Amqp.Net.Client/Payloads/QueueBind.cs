using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Utils;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class QueueBind : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(50, 20);

        internal readonly Int16 Reserved1;
        internal readonly String QueueName;
        internal readonly String ExchangeName;
        internal readonly String RoutingKey;
        internal readonly Boolean NoWait;
        internal readonly Table Arguments;

        internal QueueBind(Int16 reserved1,
                           String queueName,
                           String exchangeName,
                           String routingKey,
                           Boolean noWait,
                           Table arguments) // TODO: arguments should be a bit less "generic"
        {
            Reserved1 = reserved1;

            ValidationUtils.ValidateQueueName(queueName);
            QueueName = queueName;

            ValidationUtils.ValidateExchangeName(exchangeName);
            ExchangeName = exchangeName;

            RoutingKey = routingKey;
            NoWait = noWait;
            Arguments = arguments;
        }

        internal static QueueBind Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var queueName = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var exchangeName = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var routingKey = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var b = (Int32)buffer.ReadByte();
            var noWait = (b & 1) == 1;

            var arguments = TableFieldValueCodec.Instance.Decode(buffer);

            return new QueueBind(reserved1,
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
            ShortStringFieldValueCodec.Instance.Encode(QueueName, buffer);
            ShortStringFieldValueCodec.Instance.Encode(ExchangeName, buffer);
            ShortStringFieldValueCodec.Instance.Encode(RoutingKey, buffer);

            var b = 0;

            if (NoWait)
                b |= 1;

            buffer.WriteByte((Byte)b);

            TableFieldValueCodec.Instance.Encode(Arguments, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"queue_name\":\"{QueueName}\",\"exchange_name\":\"{ExchangeName}\",\"routing_key\":\"{RoutingKey}\",\"no_wait\":{NoWait.ToString().ToLowerInvariant()},\"arguments\":{Arguments}}}";
        }
    }
}