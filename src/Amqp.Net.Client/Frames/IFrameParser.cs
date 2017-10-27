using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal interface IFrameParser
    {
        IFrame Parse(IByteBuffer buffer);
    }
}