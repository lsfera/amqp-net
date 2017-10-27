using System;

namespace Amqp.Net.Client.Entities
{
    public enum FrameType : Byte
    {
        METHOD = 1,
        HEADER = 2,
        BODY = 3,
        OOB­METHOD = 4,
        OOB­HEADER = 5,
        OOB­BODY = 6,
        TRACE = 7,
        HEARTBEAT = 8,
        REQUEST = 9,
        RESPONSE = 10
    }
}