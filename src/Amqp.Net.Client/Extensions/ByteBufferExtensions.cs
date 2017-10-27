using System;
using Amqp.Net.Client.Decoding;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Extensions
{
    internal static class ByteBufferExtensions
    {
        internal static String DecodeFieldName(this IByteBuffer buffer)
        {
            return ShortStringFieldValueCodec.Instance.Decode(buffer);
        }

        internal static void EncodeFieldName(this String source, IByteBuffer buffer)
        {
            ShortStringFieldValueCodec.Instance.Encode(source, buffer);
        }
    }
}