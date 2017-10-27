using System.Threading.Tasks;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal interface IFrame
    {
        FrameHeader Header { get; }

        IFramePayload Payload { get; }

        Task WriteToAsync(DotNetty.Transport.Channels.IChannel channel);
    }
}