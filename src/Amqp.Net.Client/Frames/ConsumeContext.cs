using System;

namespace Amqp.Net.Client.Frames
{
    public class ConsumeContext : IFrameContext
    {
        internal ConsumeContext(IFrame frame, String consumerTag)
        {
            ChannelIndex = frame.Header.ChannelIndex;
            ConsumerTag = consumerTag;
        }

        public Int16 ChannelIndex { get; }

        internal String ConsumerTag { get; }
    }
}