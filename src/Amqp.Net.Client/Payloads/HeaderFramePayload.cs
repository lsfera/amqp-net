using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    public class HeaderFramePayload : IFramePayload
    {
        internal readonly Int16 ClassId;
        internal readonly Int16 Weight;
        internal readonly Int64 BodySize;
        internal readonly Properties Fields;

        internal HeaderFramePayload(Int16 classId,
                                    Int16 weight,
                                    Int64 bodySize,
                                    Properties fields)
        {
            ClassId = classId;
            Weight = weight;
            BodySize = bodySize;
            Fields = fields;
        }

        public void Write(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(ClassId, buffer);
            Int16FieldValueCodec.Instance.Encode(Weight, buffer);
            Int64FieldValueCodec.Instance.Encode(BodySize, buffer);
            Fields.Write(buffer);
        }

        public static HeaderFramePayload Parse(IByteBuffer buffer)
        {
            return new HeaderFramePayload(Int16FieldValueCodec.Instance.Decode(buffer),
                                          Int16FieldValueCodec.Instance.Decode(buffer),
                                          Int64FieldValueCodec.Instance.Decode(buffer),
                                          Properties.FromBuffer(buffer));
        }

        public override String ToString()
        {
            return $"{{\"class_id\":{ClassId},\"weight\":{Weight},\"body_size\":{BodySize},\"fields\":[{Fields}]}}";
        }

        internal class Properties : IEquatable<Properties>
        {
            public Boolean Equals(Properties other)
            {
                if (ReferenceEquals(null, other))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return String.Equals(ContentType, other.ContentType) &&
                       String.Equals(ContentEncoding, other.ContentEncoding) &&
                       Equals(Headers, other.Headers) &&
                       DeliveryMode == other.DeliveryMode &&
                       Priority == other.Priority &&
                       String.Equals(CorrelationId, other.CorrelationId) &&
                       String.Equals(ReplyTo, other.ReplyTo) &&
                       String.Equals(Expiration, other.Expiration) &&
                       String.Equals(MessageId, other.MessageId) &&
                       Timestamp == other.Timestamp &&
                       String.Equals(Type, other.Type) &&
                       String.Equals(UserId, other.UserId) &&
                       String.Equals(AppId, other.AppId) &&
                       String.Equals(Reserved, other.Reserved);
            }

            public override Boolean Equals(Object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                var other = obj as Properties;
                return other != null && Equals(other);
            }

            public override Int32 GetHashCode()
            {
                unchecked
                {
                    var hashCode = (ContentType != null ? ContentType.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (ContentEncoding != null ? ContentEncoding.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Headers != null ? Headers.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ DeliveryMode.GetHashCode();
                    hashCode = (hashCode * 397) ^ Priority.GetHashCode();
                    hashCode = (hashCode * 397) ^ (CorrelationId != null ? CorrelationId.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (ReplyTo != null ? ReplyTo.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Expiration != null ? Expiration.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (MessageId != null ? MessageId.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                    hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (UserId != null ? UserId.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (AppId != null ? AppId.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Reserved != null ? Reserved.GetHashCode() : 0);

                    return hashCode;
                }
            }

            public static Boolean operator ==(Properties left, Properties right)
            {
                return Equals(left, right);
            }

            public static Boolean operator !=(Properties left, Properties right)
            {
                return !Equals(left, right);
            }

            internal readonly String ContentType;
            internal readonly String ContentEncoding;
            internal readonly Table Headers;
            internal readonly Byte? DeliveryMode;
            internal readonly Byte? Priority;
            internal readonly String CorrelationId;
            internal readonly String ReplyTo;
            internal readonly String Expiration;
            internal readonly String MessageId;
            internal readonly Int64? Timestamp;
            internal readonly String Type;
            internal readonly String UserId;
            internal readonly String AppId;
            internal readonly String Reserved;

            public Properties(String contentType,
                              String contentEncoding,
                              Table headers,
                              Byte? deliveryMode,
                              Byte? priority,
                              String correlationId,
                              String replyTo,
                              String expiration,
                              String messageId,
                              Int64? timestamp,
                              String type,
                              String userId,
                              String appId,
                              String reserved)
            {
                ContentType = contentType;
                ContentEncoding = contentEncoding;
                Headers = headers;
                DeliveryMode = deliveryMode;
                Priority = priority;
                CorrelationId = correlationId;
                ReplyTo = replyTo;
                Expiration = expiration;
                MessageId = messageId;
                Timestamp = timestamp;
                Type = type;
                UserId = userId;
                AppId = appId;
                Reserved = reserved;
            }

            internal static Properties FromBuffer(IByteBuffer buffer)
            {
                var flags = Int16FieldValueCodec.Instance.Decode(buffer);

                var contentType = Decode(buffer, flags, 15, ShortStringFieldValueCodec.Instance);
                var contentEncoding = Decode(buffer, flags, 14, ShortStringFieldValueCodec.Instance);
                var headers = Decode(buffer, flags, 13, TableFieldValueCodec.Instance);
                var deliveryMode = DecodeNullable(buffer, flags, 12, ByteFieldValueCodec.Instance);
                var priority = DecodeNullable(buffer, flags, 11, ByteFieldValueCodec.Instance);
                var correlationId = Decode(buffer, flags, 10, ShortStringFieldValueCodec.Instance);
                var replyTo = Decode(buffer, flags, 9, ShortStringFieldValueCodec.Instance);
                var expiration = Decode(buffer, flags, 8, ShortStringFieldValueCodec.Instance);
                var messageId = Decode(buffer, flags, 7, ShortStringFieldValueCodec.Instance);
                var timestamp = DecodeNullable(buffer, flags, 6, Int64FieldValueCodec.Instance);
                var type = Decode(buffer, flags, 5, ShortStringFieldValueCodec.Instance);
                var userId = Decode(buffer, flags, 4, ShortStringFieldValueCodec.Instance);
                var appId = Decode(buffer, flags, 3, ShortStringFieldValueCodec.Instance);
                var reserved = Decode(buffer, flags, 2, ShortStringFieldValueCodec.Instance);

                return new Properties(contentType,
                                      contentEncoding,
                                      headers,
                                      deliveryMode,
                                      priority,
                                      correlationId,
                                      replyTo,
                                      expiration,
                                      messageId,
                                      timestamp,
                                      type,
                                      userId,
                                      appId,
                                      reserved);
            }

            internal void Write(IByteBuffer buffer)
            {
                var b = Unpooled.Buffer();
                Int16 flags = 0;

                if (ContentType != null)
                {
                    flags = (Int16)(flags | 1 << 15);
                    ShortStringFieldValueCodec.Instance.Encode(ContentType, b);
                }
                
                if (ContentEncoding != null)
                {
                    flags = (Int16)(flags | 1 << 14);
                    ShortStringFieldValueCodec.Instance.Encode(ContentEncoding, b);
                }
                
                if (Headers != null)
                {
                    flags = (Int16)(flags | 1 << 13);
                    TableFieldValueCodec.Instance.Encode(Headers, b);
                }
                
                if (DeliveryMode.HasValue)
                {
                    flags = (Int16)(flags | 1 << 12);
                    ByteFieldValueCodec.Instance.Encode(DeliveryMode.Value, b);
                }

                if (Priority.HasValue)
                {
                    flags = (Int16)(flags | 1 << 11);
                    ByteFieldValueCodec.Instance.Encode(Priority.Value, b);
                }

                if (CorrelationId != null)
                {
                    flags = (Int16)(flags | 1 << 10);
                    ShortStringFieldValueCodec.Instance.Encode(CorrelationId, b);
                }

                if (ReplyTo != null)
                {
                    flags = (Int16)(flags | 1 << 9);
                    ShortStringFieldValueCodec.Instance.Encode(ReplyTo, b);
                }

                if (Expiration != null)
                {
                    flags = (Int16)(flags | 1 << 8);
                    ShortStringFieldValueCodec.Instance.Encode(Expiration, b);
                }

                if (MessageId != null)
                {
                    flags = (Int16)(flags | 1 << 7);
                    ShortStringFieldValueCodec.Instance.Encode(MessageId, b);
                }

                if (Timestamp.HasValue)
                {
                    flags = (Int16)(flags | 1 << 6);
                    Int64FieldValueCodec.Instance.Encode(Timestamp.Value, b);
                }

                if (Type != null)
                {
                    flags = (Int16)(flags | 1 << 5);
                    ShortStringFieldValueCodec.Instance.Encode(Type, b);
                }

                if (UserId != null)
                {
                    flags = (Int16)(flags | 1 << 4);
                    ShortStringFieldValueCodec.Instance.Encode(UserId, b);
                }

                if (AppId != null)
                {
                    flags = (Int16)(flags | 1 << 3);
                    ShortStringFieldValueCodec.Instance.Encode(AppId, b);
                }

                if (Reserved != null)
                {
                    flags = (Int16)(flags | 1 << 2);
                    ShortStringFieldValueCodec.Instance.Encode(Reserved, b);
                }

                Int16FieldValueCodec.Instance.Encode(flags, buffer);
                buffer.WriteBytes(b);
            }

            private static T Decode<T>(IByteBuffer buffer,
                                       Int16 flags,
                                       Int32 index,
                                       FieldValueCodec<T> codec,
                                       T @default = default(T))
            {
                return IsBitSet(flags, index) ? codec.Decode(buffer) : @default;
            }

            private static T? DecodeNullable<T>(IByteBuffer buffer,
                                                Int16 flags,
                                                Int32 index,
                                                FieldValueCodec<T> codec)
                where T : struct
            {
                return !IsBitSet(flags, index) ? (T?)null : codec.Decode(buffer);
            }

            private static Boolean IsBitSet(Int16 b, Int32 position)
            {
                return (b & (1 << position)) != 0;
            }

            public override String ToString()
            {
                return $"{{\"content_type\":{ValueOrDefault(ContentType)},\"content_encoding\":{ValueOrDefault(ContentEncoding)},\"headers\":{ValueOrDefault(Headers)},\"delivery_mode\":{ValueOrDefault(DeliveryMode)},\"priority\":{ValueOrDefault(Priority)},\"correlation_id\":{ValueOrDefault(CorrelationId)},\"reply_to\":{ValueOrDefault(ReplyTo)},\"expiration\":{ValueOrDefault(Expiration)},\"message_id\":{ValueOrDefault(MessageId)},\"timestamp\":{ValueOrDefault(Timestamp)},\"type\":{ValueOrDefault(Type)},\"user_id\":{ValueOrDefault(UserId)},\"app_id\":{ValueOrDefault(AppId)},\"reserved\":{ValueOrDefault(Reserved)}}}";
            }

            private static String ValueOrDefault(String source)
            {
                return source != null ? $"\"{source}\"" : "null";
            }

            private static String ValueOrDefault(Table source)
            {
                return source != null ? $"{source}" : "null";
            }

            private static String ValueOrDefault<T>(T? source)
                where T : struct
            {
                return source.HasValue ? $"{source.ToString()}" : "null";
            }
        }
    }
}