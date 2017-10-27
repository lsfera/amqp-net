using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amqp.Net.Client.Extensions;

namespace Amqp.Net.Client
{
    // TODO: add tls support
    public struct ConnectionString
    {
        private const Int32 DefaultPort = 5672;
        private const String DefaultVirtualHost = "/";

        public readonly IPEndPoint Endpoint;
        public readonly NetworkCredential Credentials;
        public readonly String VirtualHost;

        public ConnectionString(IPEndPoint endpoint,
                                NetworkCredential credentials,
                                String virtualHost)
        {
            Endpoint = endpoint;
            Credentials = credentials;
            VirtualHost = virtualHost;
        }

        public static Task<ConnectionString> ParseAsync(String connectionString)
        {
            var uri = new Uri(connectionString, UriKind.Absolute);
            // TODO: check for valid host

            return Dns.GetHostAddressesAsync(uri.Host)
                      .Then(_ =>
                            {
                                if (_.Length == 0)
                                    throw new Exception($"cannot resolve host {uri.Host}"); // TODO: make use of ad-hoc exception

                                var address = Enumerable.First<IPAddress>(_); // TODO: should we just take the first one?
                                var endpoint = new IPEndPoint(address, uri.Port != -1
                                                                  ? uri.Port
                                                                  : DefaultPort);

                                return new ConnectionString(endpoint,
                                                            ParseCredentials(uri),
                                                            uri.Segments.Length == 2
                                                                ? UriDecode(uri.Segments[1])
                                                                : DefaultVirtualHost);
                            });
        }

        private static NetworkCredential ParseCredentials(Uri uri)
        {
            var userInfo = (uri.UserInfo ?? String.Empty);
            var index = userInfo.IndexOf(':');

            return index == -1
                       ? new NetworkCredential(userInfo, String.Empty)
                       : new NetworkCredential(userInfo.Substring(0, index),
                                               userInfo.Substring(index + 1));
        }

        private static String UriDecode(String url)
        {
            return Uri.UnescapeDataString(url.Replace("+", "%2B"));
        }
    }
}