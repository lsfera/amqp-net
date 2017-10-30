using System;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    internal class ProtocolHeader : IFramePayload
    {
        public void Write(IByteBuffer buffer)
        {
            buffer.WriteBytes(new Byte[] { 0x41, 0x4d, 0x51, 0x50, 0x01, 0x01, 0x00, 0x09 });
        }
    }
}