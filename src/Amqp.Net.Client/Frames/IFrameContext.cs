using System;

namespace Amqp.Net.Client.Frames
{
    internal interface IFrameContext
    {
        Int16 ChannelIndex { get; }
    }
}