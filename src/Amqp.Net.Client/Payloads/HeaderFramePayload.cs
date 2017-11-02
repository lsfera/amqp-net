using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class HeaderFramePayload : IFramePayload
    {
        internal readonly Int16 ClassId;
        internal readonly Int16 Weight;
        internal readonly Int64 BodySize;
        internal readonly Int16 PropertyFlags;
        internal readonly Table PropertyList;

        private HeaderFramePayload(Int16 classId,
                                   Int16 weight,
                                   Int64 bodySize,
                                   Int16 propertyFlags,
                                   Table propertyList)
        {
            ClassId = classId;
            Weight = weight;
            BodySize = bodySize;
            PropertyFlags = propertyFlags;
            PropertyList = propertyList;
        }

        public void Write(IByteBuffer buffer)
        {
            // TODO
            throw new NotImplementedException();
        }

        public static HeaderFramePayload Parse(IByteBuffer buffer)
        {
            var classId = Int16FieldValueCodec.Instance.Decode(buffer);
            var weight = Int16FieldValueCodec.Instance.Decode(buffer);
            var bodySize = Int64FieldValueCodec.Instance.Decode(buffer);
            var propertyFlags = Int16FieldValueCodec.Instance.Decode(buffer);
            var propertyList = TableFieldValueCodec.Instance.Decode(buffer);

            return new HeaderFramePayload(classId,
                                          weight,
                                          bodySize,
                                          propertyFlags,
                                          propertyList);
        }
    }
}