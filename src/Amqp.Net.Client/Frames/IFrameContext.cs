using System;

namespace Amqp.Net.Client.Frames
{
    public interface IFrameContext
    {
        Int16 ChannelIndex { get; }
    }
}