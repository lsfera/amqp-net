using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    public class HeaderPayload : IFramePayload
    {
        internal readonly Int16 ClassId;
        internal readonly Int16 Weight;
        internal readonly Int64 BodySize;
        internal readonly ConsumedMessageProperties Fields;

        internal HeaderPayload(Int16 classId,
                               Int16 weight,
                               Int64 bodySize,
                               ConsumedMessageProperties fields)
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

        public static HeaderPayload Parse(IByteBuffer buffer)
        {
            return new HeaderPayload(Int16FieldValueCodec.Instance.Decode(buffer),
                                     Int16FieldValueCodec.Instance.Decode(buffer),
                                     Int64FieldValueCodec.Instance.Decode(buffer),
                                     ConsumedMessageProperties.FromBuffer(buffer));
        }

        public override String ToString()
        {
            return $"{{\"class_id\":{ClassId},\"weight\":{Weight},\"body_size\":{BodySize},\"fields\":[{Fields}]}}";
        }
    }
}