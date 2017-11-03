using System;

namespace Amqp.Net.Client.Frames
{
    internal class EmptyContext : IFrameContext
    {
        internal EmptyContext(IFrame frame)
        {
            ChannelIndex = frame.Header.ChannelIndex;
        }

        public Int16 ChannelIndex { get; }
    }
}