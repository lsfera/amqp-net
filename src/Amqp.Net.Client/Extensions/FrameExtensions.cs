using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Extensions
{
    internal static class FrameExtensions
    {
        internal static Task<T> SendAsync<T>(this T input, DotNetty.Transport.Channels.IChannel channel)
            where T : IFrame
        {
            return SendAsync(input, channel, _ => _);
        }

        internal static Task<TOutput> SendAsync<TInput, TOutput>(this TInput input,
                                                                 DotNetty.Transport.Channels.IChannel channel,
                                                                 Func<TInput, TOutput> func)
            where TInput : IFrame
            where TOutput : IFrame
        {
            var frame = func(input);

            return frame.WriteToAsync(channel)
                        .Then(() => frame);
        }

        internal static Task<T> Cast<T>(this Task<IFrame> task)
            where T : class, IFrame
        {
            return task.Then(_ => _ as T);
        }

        internal static ConnectionOpenFrame ToConnectionOpenFrame(this ConnectionTuneOkFrame frame,
                                                                  String virtualHost)
        {
            return new ConnectionOpenFrame(frame.Header.ChannelIndex,
                                           new ConnectionOpenPayload(virtualHost));
        }

        internal static ConnectionStartOkFrame ToConnectionStartOkFrame(this ConnectionStartFrame frame,
                                                                        NetworkCredential credentials)
        {
            // TODO: check te server AMQP version

            // TODO: "connection_name" should be configured in some way
            const String connectionName = "hello_amqp";

            var properties = new ClientProperties(ClientCapabilities.FromServerCapabilities(frame.Payload),
                                                  connectionName);
            var mechanism = frame.Payload
                                 .Mechanisms
                                 .FirstOrDefault(_ => AuthMechanismMap.ContainsKey(_));

            if (mechanism == null)
                throw new Exception("could not find a supported authentication mechanisms"); // TODO: ad-hoc exception

            var locale = frame.Payload
                              .Locales
                              .FirstOrDefault(_ => AvailableLocales.Contains(_));

            if (locale == null)
                throw new Exception("could not find a supported locale"); // TODO: ad-hoc exception

            return new ConnectionStartOkFrame(frame.Header.ChannelIndex,
                                              new ConnectionStartOkPayload(properties,
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