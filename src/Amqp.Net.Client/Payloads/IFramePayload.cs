using DotNetty.Buffers;

namespace Amqp.Net.Client.Payloads
{
    public interface IFramePayload
    {
        void Write(IByteBuffer buffer);
    }
}