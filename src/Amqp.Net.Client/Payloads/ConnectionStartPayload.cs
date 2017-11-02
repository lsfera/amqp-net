using System;
using System.Collections.Generic;
using System.Linq;
using Amqp.Net.Client.Decoding;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ConnectionStartPayload : MethodFramePayload
    {
        internal static readonly MethodFrameDescriptor StaticDescriptor = new MethodFrameDescriptor(10, 10);

        internal readonly AmqpVersion Version;
        internal readonly Table ServerProperties;
        internal readonly String[] Mechanisms;
        internal readonly String[] Locales;

        internal static ConnectionStartPayload Parse(IByteBuffer buffer)
        {
            return new ConnectionStartPayload(AmqpVersion.New(buffer.ReadByte(), buffer.ReadByte()),
                                              TableFieldValueCodec.Instance.Decode(buffer),
                                              Split(LongStringFieldValueCodec.Instance.Decode(buffer)),
                                              Split(LongStringFieldValueCodec.Instance.Decode(buffer)));
        }

        internal ConnectionStartPayload(AmqpVersion version,
                                        Table serverProperties,
                                        String[] mechanisms,
                                        String[] locales)
        {
            Version = version;
            ServerProperties = serverProperties ?? new Table(new Dictionary<String, Object>());
            Mechanisms = mechanisms;
            Locales = locales;
        }

        internal override MethodFrameDescriptor Descriptor => StaticDescriptor;

        protected override void WriteInternal(IByteBuffer buffer)
        {
            ByteFieldValueCodec.Instance.Encode(Version.Major, buffer);
            ByteFieldValueCodec.Instance.Encode(Version.Minor, buffer);
            TableFieldValueCodec.Instance.Encode(ServerProperties, buffer);
            LongStringFieldValueCodec.Instance.Encode(Merge(Mechanisms), buffer);
            LongStringFieldValueCodec.Instance.Encode(Merge(Locales), buffer);
        }

        private static String Merge(String[] source, String separator = " ")
        {
            return String.Join(separator, source);
        }

        private static String ToString(IEnumerable<String> source)
        {
            return String.Join(",", source.Select(_ => $"\"{_}\""));
        }

        private static String[] Split(String source, Char separator = ' ')
        {
            return source.Split(new[] { separator },
                                StringSplitOptions.RemoveEmptyEntries);
        }

        public override String ToString()
        {
            return $"{{\"descriptor\":{Descriptor},\"amqp_version\":\"{Version}\",\"server_properties\":{ServerProperties},\"mechanisms\":[{ToString(Mechanisms)}],\"locales\":[{ToString(Locales)}]}}";
        }
    }
}