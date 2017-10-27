using System;
using System.Collections.Generic;

namespace Amqp.Net.Client.Entities
{
    internal class ClientProperties : Table
    {
        internal ClientProperties(ClientCapabilities capabilities,
                                  String connectionName)
            : base(new Dictionary<String, Object>
                       {
                           { "capabilities", capabilities },
                           { "connection_name", connectionName}
                       })
        {
        }

        internal ClientCapabilities Capabilities => (ClientCapabilities)Fields["capabilities"];

        internal String ConnectionName => (String)Fields["connection_name"];
    }
}