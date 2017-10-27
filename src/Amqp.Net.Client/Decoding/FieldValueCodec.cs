using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal abstract class FieldValueCodec<T> : IFieldValueCodec
    {
        internal abstract T Decode(IByteBuffer buffer);

        internal abstract void Encode(T source, IByteBuffer buffer);

        Object IFieldValueCodec.Decode(IByteBuffer buffer)
        {
            return Decode(buffer);
        }

        void IFieldValueCodec.Encode(Object source, IByteBuffer buffer)
        {
            Encode((T)source, buffer);
        }

        public abstract Byte Type { get; }
    }
}