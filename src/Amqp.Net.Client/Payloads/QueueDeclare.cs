using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Utils;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class QueueDeclare : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(50, 10);

        internal readonly Int16 Reserved1;
        internal readonly String Name;
        internal readonly Boolean Passive;

        /// <remarks>
        /// If set when creating a new exchange, the exchange
        /// will be marked as durable. Durable exchanges
        /// remain active when a server restarts.
        /// Non-durable exchanges (transient exchanges)
        /// are purged if/when a server restarts.
        /// </remarks>
        internal readonly Boolean Durable;

        /// <summary>
        /// Exclusive queues may only be accessed by the
        /// current connection, and are deleted when that
        /// connection closes.
        /// </summary>
        internal readonly Boolean Exclusive;

        /// <remarks>
        /// If set, the exchange is deleted when all
        /// queues have finished using it.
        /// </remarks>
        internal readonly Boolean AutoDelete; // NOTE: might be useful for certain scenarios...

        internal readonly Boolean NoWait;
        internal readonly Table Arguments;

        internal QueueDeclare(Int16 reserved1,
                              String name,
                              Boolean passive,
                              Boolean durable,
                              Boolean exclusive,
                              Boolean autoDelete,
                              Boolean noWait,
                              Table arguments) // TODO: arguments should be a bit less "generic"
        {
            Reserved1 = reserved1;

            ValidationUtils.ValidateQueueName(name);
            Name = name;

            Passive = passive;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            NoWait = noWait;
            Arguments = arguments;
        }

        internal static QueueDeclare Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var name = ShortStringFieldValueCodec.Instance.Decode(buffer);

            var b = (Int32)buffer.ReadByte();
            var passive = (b & 1) == 1;
            var durable = (b & 2) == 2;
            var exclusive = (b & 4) == 4;
            var autoDelete = (b & 8) == 8;
            var noWait = (b & 16) == 16;

            var arguments = TableFieldValueCodec.Instance.Decode(buffer);

            return new QueueDeclare(reserved1,
                                    name,
                                    passive,
                                    durable,
                                    exclusive,
                                    autoDelete,
                                    noWait,
                                    arguments);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(Reserved1, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Name, buffer);

            var b = 0;

            if (Passive)
                b |= 1;

            if (Durable)
                b |= 2;

            if (Exclusive)
                b |= 4;

            if (AutoDelete)
                b |= 8;

            if (NoWait)
                b |= 16;

            buffer.WriteByte((Byte)b);

            TableFieldValueCodec.Instance.Encode(Arguments, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"name\":\"{Name}\",\"passive\":{Passive.ToString().ToLowerInvariant()},\"durable\":{Durable.ToString().ToLowerInvariant()},\"exclusive\":{Exclusive.ToString().ToLowerInvariant()},\"auto_delete\":{AutoDelete.ToString().ToLowerInvariant()},\"no_wait\":{NoWait.ToString().ToLowerInvariant()},\"arguments\":{Arguments}}}";
        }
    }
}