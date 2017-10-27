﻿using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class QueueDeleteOk : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(50, 41);

        internal readonly Int32 MessageCount;

        internal static QueueDeleteOk Parse(IByteBuffer buffer)
        {
            return new QueueDeleteOk(Int32FieldValueCodec.Instance.Decode(buffer));
        }

        internal QueueDeleteOk(Int32 messageCount)
        {
            MessageCount = messageCount;
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            Int32FieldValueCodec.Instance.Encode(MessageCount, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"message_count\":{MessageCount}}}";
        }
    }
}