using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class BasicConsume : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(60, 20);

        internal readonly Int16 Reserved1;
        internal readonly String QueueName;
        internal readonly String ConsumerTag;

        /// <remarks>
        /// If the no-local field is set the server will not
        /// send messages to the connection that published them.
        /// </remarks>
        internal readonly Boolean NoLocal;

        /// <remarks>
        /// If this field is set the server does not expect
        /// acknowledgements for messages.
        /// </remarks>
        internal readonly Boolean NoAck;

        /// <remarks>
        /// Request exclusive consumer access, meaning only
        /// this consumer can access the queue.
        /// </remarks>
        internal readonly Boolean Exclusive;

        internal readonly Boolean NoWait;
        internal readonly Table Arguments;

        internal BasicConsume(Int16 reserved1,
                              String queueName,
                              String consumerTag,
                              Boolean noLocal,
                              Boolean noAck,
                              Boolean exclusive,
                              Boolean noWait,
                              Table arguments) // TODO: arguments should be a bit less "generic"
        {
            Reserved1 = reserved1;
            QueueName = queueName;
            ConsumerTag = consumerTag;
            NoLocal = noLocal;
            NoAck = noAck;
            Exclusive = exclusive;
            NoWait = noWait;
            Arguments = arguments;
        }

        internal static BasicConsume Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var queueName = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var consumerTag = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var b = (Int32)buffer.ReadByte();
            var noLocal = (b & 1) == 1;
            var noAck = (b & 2) == 2;
            var exclusive = (b & 4) == 4;
            var noWait = (b & 8) == 8;

            var arguments = TableFieldValueCodec.Instance.Decode(buffer);

            return new BasicConsume(reserved1,
                                    queueName,
                                    consumerTag,
                                    noLocal,
                                    noAck,
                                    exclusive,
                                    noWait,
                                    arguments);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(Reserved1, buffer);
            ShortStringFieldValueCodec.Instance.Encode(QueueName, buffer);
            ShortStringFieldValueCodec.Instance.Encode(ConsumerTag, buffer);

            var b = 0;

            if (NoLocal)
                b |= 1;

            if (NoAck)
                b |= 2;

            if (Exclusive)
                b |= 4;

            if (NoWait)
                b |= 8;

            buffer.WriteByte((Byte)b);

            TableFieldValueCodec.Instance.Encode(Arguments, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"queue_name\":\"{QueueName}\",\"consumer_tag\":\"{ConsumerTag}\",\"no_local\":{NoLocal},\"no_ack\":{NoAck},\"exclusive\":{Exclusive},\"no_wait\":{NoWait},\"arguments\":{Arguments}}}";
        }
    }
}