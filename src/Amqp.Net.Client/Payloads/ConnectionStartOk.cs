using System;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ConnectionStartOk : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(10, 11);

        internal readonly Table ClientProperties;
        internal readonly String Mechanism;
        internal readonly String Response;
        internal readonly String Locale;

        internal static ConnectionStartOk Parse(IByteBuffer buffer)
        {
            return new ConnectionStartOk(TableFieldValueCodec.Instance.Decode(buffer),
                                         ShortStringFieldValueCodec.Instance.Decode(buffer),
                                         LongStringFieldValueCodec.Instance.Decode(buffer),
                                         ShortStringFieldValueCodec.Instance.Decode(buffer));
        }

        internal ConnectionStartOk(Table clientProperties,
                                   String mechanism,
                                   String response,
                                   String locale)
        {
            ClientProperties = clientProperties;
            Mechanism = mechanism;
            Response = response;
            Locale = locale;
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            TableFieldValueCodec.Instance.Encode(ClientProperties, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Mechanism, buffer);
            LongStringFieldValueCodec.Instance.Encode(Response, buffer);
            ShortStringFieldValueCodec.Instance.Encode(Locale, buffer);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"client_properties\":{ClientProperties},\"mechanism\":\"{Mechanism}\",\"response\":\"{Response}\",\"locale\":\"{Locale}\"}}";
        }
    }
}