using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionStartFrame : MethodFrame<ConnectionStart, RpcContext>
    {
        internal ConnectionStartFrame(Int16 channel, ConnectionStart payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);

        // TODO: make an extension method?
        internal ConnectionStartOkFrame ToConnectionStartOkFrame(NetworkCredential credentials)
        {
            // TODO: check te server AMQP version

            // TODO: "connection_name" should be configured in some way
            const String connectionName = "hello_amqp";

            var properties = new ClientProperties(ClientCapabilities.FromServerCapabilities(Payload),
                                                  connectionName);
            var mechanism = Payload.Mechanisms
                                   .FirstOrDefault(_ => AuthMechanismMap.ContainsKey(_));

            if (mechanism == null)
                throw new Exception("could not find a supported authentication mechanisms"); // TODO: ad-hoc exception

            var locale = Payload.Locales
                                .FirstOrDefault(_ => AvailableLocales.Contains(_));

            if (locale == null)
                throw new Exception("could not find a supported locale"); // TODO: ad-hoc exception

            return new ConnectionStartOkFrame(Header.ChannelIndex,
                                              new ConnectionStartOk(properties,
                                                                    mechanism,
                                                                    AuthMechanismMap[mechanism](credentials),
                                                                    locale));
        }

        // TODO: configure by settings
        private static readonly IDictionary<String, Func<NetworkCredential, String>> AuthMechanismMap =
            new Dictionary<String, Func<NetworkCredential, String>>
                {
                    { "PLAIN", _ => $"\0{_.UserName}\0{_.Password}" }
                };

        // TODO: configure by settings
        private static readonly ISet<String> AvailableLocales = new HashSet<String> { "en_US" };
    }
}