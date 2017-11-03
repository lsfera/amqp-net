using System;

namespace Amqp.Net.Client.Entities
{
    public struct ConsumedMessage
    {
        public readonly ConsumedMessageProperties Properties;
        public readonly Byte[] Body;

        internal ConsumedMessage(ConsumedMessageProperties properties,
                                 Byte[] body)
        {
            Properties = properties;
            Body = body;
        }
    }
}