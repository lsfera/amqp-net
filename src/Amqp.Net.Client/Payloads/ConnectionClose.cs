using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ConnectionClose : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(10, 50);

        internal readonly Int16 ReplyCode;
        internal readonly String ReplyText;
        internal readonly Int16 SourceClassId;
        internal readonly Int16 SourceMethodId;

        internal ConnectionClose(Int16 replyCode,
                                 String replyText,
                                 Int16 sourceClassId,
                                 Int16 sourceMethodId)
        {
            ReplyCode = replyCode;
            ReplyText = replyText;
            SourceClassId = sourceClassId;
            SourceMethodId = sourceMethodId;
        }

        internal static ConnectionClose Parse(IByteBuffer buffer)
        {
            return new ConnectionClose(Int16FieldValueCodec.Instance.Decode(buffer),
                                       ShortStringFieldValueCodec.Instance.Decode(buffer),
                                       Int16FieldValueCodec.Instance.Decode(buffer),
                                       Int16FieldValueCodec.Instance.Decode(buffer));
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(ReplyCode, buffer);
            ShortStringFieldValueCodec.Instance.Encode(ReplyText, buffer);
            Int16FieldValueCodec.Instance.Encode(SourceClassId, buffer);
            Int16FieldValueCodec.Instance.Encode(SourceMethodId, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"reply_code\":{ReplyCode},\"reply_text\":\"{ReplyText}\",\"class_id\":{SourceClassId},\"source_method_id\":{SourceMethodId}}}";
        }
    }
}