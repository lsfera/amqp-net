using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Utils;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class QueueUnbind : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(50, 50);

        internal readonly Int16 Reserved1;
        internal readonly String QueueName;
        internal readonly String ExchangeName;
        internal readonly String RoutingKey;
        internal readonly Table Arguments;

        internal QueueUnbind(Int16 reserved1,
                             String queueName,
                             String exchangeName,
                             String routingKey,
                             Table arguments) // TODO: arguments should be a bit less "generic"
        {
            Reserved1 = reserved1;

            ValidationUtils.ValidateQueueName(queueName);
            QueueName = queueName;

            ValidationUtils.ValidateExchangeName(exchangeName);
            ExchangeName = exchangeName;

            RoutingKey = routingKey;
            Arguments = arguments;
        }

        internal static QueueUnbind Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var queueName = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var exchangeName = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var routingKey = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var arguments = TableFieldValueCodec.Instance.Decode(buffer);

            return new QueueUnbind(reserved1,
                                   queueName,
                                   exchangeName,
                                   routingKey,
                                   arguments);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(Reserved1, buffer);
            ShortStringFieldValueCodec.Instance.Encode(QueueName, buffer);
            ShortStringFieldValueCodec.Instance.Encode(ExchangeName, buffer);
            ShortStringFieldValueCodec.Instance.Encode(RoutingKey, buffer);
            TableFieldValueCodec.Instance.Encode(Arguments, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"queue_name\":\"{QueueName}\",\"exchange_name\":\"{ExchangeName}\",\"routing_key\":\"{RoutingKey}\",\"arguments\":{Arguments}}}";
        }
    }
}