using System;
using System.Threading.Tasks;

namespace Amqp.Net.Client
{
    public interface IConnection : IDisposable
    {
        Task<IChannel> OpenChannelAsync();

        Task CloseAsync();
    }
}