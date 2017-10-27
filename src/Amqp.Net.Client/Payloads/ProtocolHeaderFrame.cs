using System;
using System.Threading.Tasks;
using Amqp.Net.Client.Extensions;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;

namespace Amqp.Net.Client.Payloads
{
    internal class ProtocolHeader
    {
        internal static readonly ProtocolHeader Instance = new ProtocolHeader();

        private static readonly Byte[] data = { 0x41, 0x4d, 0x51, 0x50, 0x01, 0x01, 0x00, 0x09 };

        internal Task<Byte[]> WriteToAsync(DotNetty.Transport.Channels.IChannel channel)
        {
            var buffer = Unpooled.WrappedBuffer(data);

            return channel.WriteAndFlushAsync(buffer)
                          .Then(() =>
                                {
                                    buffer.SafeRelease();
                          
                                    return data;
                                });
        }
    }
}