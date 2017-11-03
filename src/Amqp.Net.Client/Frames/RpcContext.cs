using System;

namespace Amqp.Net.Client.Frames
{
    internal class RpcContext : IFrameContext
    {
        internal RpcContext(IFrame frame)
        {
            ChannelIndex = frame.Header.ChannelIndex;
        }

        public Int16 ChannelIndex { get; }
    }
}