using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    public interface IFrame
    {
        FrameHeader Header { get; }

        IFramePayload Payload { get; }

        IFrameContext Context { get; }

        Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel);
    }
}