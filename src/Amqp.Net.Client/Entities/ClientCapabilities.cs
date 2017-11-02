using System;
using System.Collections.Generic;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Entities
{
    internal class ClientCapabilities : Table
    {
        internal static ClientCapabilities FromServerCapabilities(ConnectionStartPayload payload)
        {
            var capabilities = payload.ServerProperties.Field<Table>("capabilities") ?? new Table(new Dictionary<String, Object>());

            return new ClientCapabilities(capabilities.Field<Boolean>("publisher_confirms"),
                                          capabilities.Field<Boolean>("exchange_exchange_bindings"),
                                          capabilities.Field<Boolean>("basic.nack"),
                                          capabilities.Field<Boolean>("consumer_cancel_notify"),
                                          capabilities.Field<Boolean>("connection.blocked"),
                                          capabilities.Field<Boolean>("authentication_failure_close"),
                                          capabilities.Field<Boolean>("per_consumer_qos"),
                                          capabilities.Field<Boolean>("consumer_priorities"));
        }

        internal ClientCapabilities(Boolean publisherConfirms,
                                    Boolean exchangeToExchangeBindings,
                                    Boolean basicNack,
                                    Boolean consumerCancelNotify,
                                    Boolean connectionBlocked,
                                    Boolean authenticationFailureClose,
                                    Boolean perConsumerQos,
                                    Boolean consumerPriorities)
            : base(new Dictionary<String, Object>
                       {
                           { "publisher_confirms", publisherConfirms },
                           { "exchange_exchange_bindings", exchangeToExchangeBindings },
                           { "basic.nack", basicNack },
                           { "consumer_cancel_notify", consumerCancelNotify },
                           { "connection.blocked", connectionBlocked },
                           { "authentication_failure_close", authenticationFailureClose },
                           { "per_consumer_qos", perConsumerQos },
                           { "consumer_priorities", consumerPriorities }
                       })
        {
        }

        internal Boolean PublisherConfirms => (Boolean)Fields["publisher_confirms"];

        internal Boolean ExchangeToExchangeBindings => (Boolean)Fields["exchange_exchange_bindings"];

        internal Boolean BasicNack => (Boolean)Fields["basic.nack"];

        internal Boolean ConsumerCancelNotify => (Boolean)Fields["consumer_cancel_notify"];

        internal Boolean ConnectionBlocked => (Boolean)Fields["connection.blocked"];

        internal Boolean AuthenticationFailureClose => (Boolean)Fields["authentication_failure_close"];

        internal Boolean PerConsumerQos => (Boolean)Fields["per_consumer_qos"];

        internal Boolean ConsumerPriorities => (Boolean)Fields["consumer_priorities"];
    }
}