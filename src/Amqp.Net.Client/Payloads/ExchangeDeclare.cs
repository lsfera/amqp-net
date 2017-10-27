using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Utils;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ExchangeDeclare : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(40, 10);

        internal readonly Int16 Reserved1;
        internal readonly String Name;
        internal readonly ExchangeType Type;
        internal readonly Boolean Passive;

        /// <remarks>
        /// If set when creating a new exchange, the exchange
        /// will be marked as durable. Durable exchanges
        /// remain active when a server restarts.
        /// Non-durable exchanges (transient exchanges)
        /// are purged if/when a server restarts.
        /// </remarks>
        internal readonly Boolean Durable;

        /// <remarks>
        /// If set, the exchange is deleted when all
        /// queues have finished using it.
        /// </remarks>
        internal readonly Boolean AutoDelete; // NOTE: might be useful for certain scenarios...

        /// <remarks>
        /// If set, the exchange may not be used directly by
        /// publishers, but only when bound to other exchanges.
        /// Internal exchanges are used to construct wiring
        /// that is not visible to applications.
        /// </remarks>
        internal readonly Boolean Internal; // NOTE: might be useful for certain scenarios...
        internal readonly Boolean NoWait;
        internal readonly Table Arguments;

        internal ExchangeDeclare(Int16 reserved1,
                                 String name,
                                 ExchangeType type,
                                 Boolean passive,
                                 Boolean durable,
                                 Boolean autoDelete,
                                 Boolean @internal,
                                 Boolean noWait,
                                 Table arguments) // TODO: arguments should be a bit less "generic"
        {
            Reserved1 = reserved1;

            ValidationUtils.ValidateExchangeName(name);
            Name = name;

            Type = type;
            Passive = passive;
            Durable = durable;
            AutoDelete = autoDelete;
            Internal = @internal;
            NoWait = noWait;
            Arguments = arguments;
        }

        internal static ExchangeDeclare Parse(IByteBuffer buffer)
        {
            var reserved1 = Int16FieldValueCodec.Instance.Decode(buffer);
            var name = ShortStringFieldValueCodec.Instance.Decode(buffer);
            var type = (ExchangeType)Enum.Parse(typeof(ExchangeType),
                                                ShortStringFieldValueCodec.Instance.Decode(buffer),
                                                true);
            var b = (Int32)buffer.ReadByte();
            var passive = (b & 1) == 1;
            var durable = (b & 2) == 2;
            var autoDelete = (b & 4) == 4;
            var @internal = (b & 8) == 8;
            var noWait = (b & 16) == 16;

            var arguments = TableFieldValueCodec.Instance.Decode(buffer);

            return new ExchangeDeclare(reserved1,
                                       name,
                                       type,
                                       passive,
                                       durable,
                                       autoDelete,
                                       @internal,
                                       noWait,
                                       arguments);
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(Reserved1, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Name, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Type.ToString().ToLowerInvariant(), buffer);

            var b = 0;

            if (Passive)
                b |= 1;

            if (Durable)
                b |= 2;

            if (AutoDelete)
                b |= 4;

            if (Internal)
                b |= 8;

            if (NoWait)
                b |= 16;

            buffer.WriteByte((Byte)b);

            TableFieldValueCodec.Instance.Encode(Arguments, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reserved_1\":{Reserved1},\"name\":\"{Name}\",\"type\":\"{Type.ToString().ToLowerInvariant()}\",\"passive\":{Passive.ToString().ToLowerInvariant()},\"durable\":{Durable.ToString().ToLowerInvariant()},\"auto_delete\":{AutoDelete.ToString().ToLowerInvariant()},\"internal\":{Internal.ToString().ToLowerInvariant()},\"no_wait\":{NoWait.ToString().ToLowerInvariant()},\"arguments\":{Arguments}}}";
        }
    }
}