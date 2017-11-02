using System;
using System.Threading;
using System.Threading.Tasks;
using Amqp.Net.Client.Extensions;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Handlers;
using Amqp.Net.Client.Payloads;
using DotNetty.Common.Concurrency;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Amqp.Net.Client
{
    public class Connection : IConnection
    {
        private readonly IEventExecutorGroup group;
        private readonly DotNetty.Transport.Channels.IChannel channel;
        private readonly IMethodFrameBag bag;
        private Int32 channelIndex;

        private Connection(IEventExecutorGroup group,
                           DotNetty.Transport.Channels.IChannel channel,
                           IMethodFrameBag bag,
                           Int16 channelIndex)
        {
            this.group = group;
            this.channel = channel;
            this.bag = bag;
            this.channelIndex = channelIndex;
        }

        public static async Task<IConnection> ConnectAsync(ConnectionString connectionString)
        {
            var group = new MultithreadEventLoopGroup();
            var bag = new MethodFrameBag();
            var parser = new FrameParser();
            var handler = new ActionChannelInitializer<ISocketChannel>(_ =>
                                                                       {
                                                                           var pipeline = _.Pipeline;
                                                                           pipeline.AddLast(new LoggingHandler());
                                                                           pipeline.AddLast(new MessageHandler(bag, parser));
                                                                       });
            var channel = await new Bootstrap().Group(group)
                                               .Channel<TcpSocketChannel>()
                                               .Option(ChannelOption.AutoRead, true)
                                               .Option(ChannelOption.TcpNodelay, true)
                                               .Option(ChannelOption.SoKeepalive, true)
                                               .Handler(handler)
                                               .ConnectAsync(connectionString.Endpoint); // TODO: handle error

            return await ProtocolHeaderFrame.Instance
                                            .SendAsync(channel)
                                            .Then(_ => bag.OnRpc(ConnectionStartPayload.StaticDescriptor)
                                                          .Register<ConnectionStartFrame>(_.Context))
                                            .Log(_ => $"RECEIVED: {_.ToString()}")
                                            .Then(_ => _.SendAsync(channel, __ => __.ToConnectionStartOkFrame(connectionString.Credentials)))
                                            .Log(_ => $"SENT: {_.ToString()}")
                                            .Then(_ => bag.OnRpc(ConnectionTunePayload.StaticDescriptor)
                                                          .Register<ConnectionTuneFrame>(_.Context))
                                            .Log(_ => $"RECEIVED: {_.ToString()}")
                                            .Then(_ => _.SendAsync(channel, __ => __.ToConnectionTuneOkFrame()))
                                            .Log(_ => $"SENT: {_.ToString()}")
                                            .Then(_ => _.SendAsync(channel, __ => __.ToConnectionOpenFrame(connectionString.VirtualHost)))
                                            .Log(_ => $"SENT: {_.ToString()}")
                                            .Then(_ => bag.OnRpc(ConnectionOpenOkPayload.StaticDescriptor)
                                                          .Register<ConnectionOpenOkFrame>(_.Context))
                                            .Log(_ => $"RECEIVED: {_.ToString()}")
                                            .Then(_ => new Connection(group, channel, bag, _.Header.ChannelIndex))
                                            .LogError();
        }

        public Task<IChannel> OpenChannelAsync()
        {
            var index = (Int16)Interlocked.Increment(ref channelIndex);

            return new ChannelOpenFrame(index,
                                        new ChannelOpenPayload()).SendAsync(channel)
                                                          .Log(_ => $"SENT: {_.ToString()}")
                                                          .Then(_ => bag.OnRpc(ChannelOpenOkPayload.StaticDescriptor)
                                                                        .Register<ChannelOpenOkFrame>(_.Context))
                                                          .Log(_ => $"RECEIVED: {_.ToString()}")
                                                          .ContinueWith<IChannel>(_ => new Channel(channel, bag, index))
                                                          .LogError();
        }

        public Task CloseAsync()
        {
            return ConnectionCloseFrame.Close()
                                       .SendAsync(channel)
                                       .Log(_ => $"SENT: {_.ToString()}")
                                       .Then(_ => bag.OnRpc(ConnectionCloseOkPayload.StaticDescriptor)
                                                     .Register<ConnectionCloseOkFrame>(_.Context))
                                       .Log(_ => $"RECEIVED: {_.ToString()}")
                                       .LogError();
        }

        public void Dispose()
        {
            try
            {
                CloseAsync().Then(() => channel.CloseAsync())
                            .Then(() => group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100),
                                                                      TimeSpan.FromSeconds(1)))
                            .LogError()
                            .Unwrap()
                            .Wait(TimeSpan.FromSeconds(5d));
            }
            catch { }
        }
    }
}