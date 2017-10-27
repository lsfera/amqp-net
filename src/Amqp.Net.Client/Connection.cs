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
                                               .Option(ChannelOption.TcpNodelay, true)
                                               .Option(ChannelOption.SoKeepalive, true)
                                               .Handler(handler)
                                               .ConnectAsync(connectionString.Endpoint); // TODO: handle error

            return await ProtocolHeader.Instance
                                       .WriteToAsync(channel)
                                       .LogError()
                                       .Then(() => bag.For(ConnectionStart.StaticDescriptor)
                                                      .WaitForAsync<ConnectionStartFrame>(0))
                                       .LogError()
                                       .Log(_ => $"RECEIVED: {_.ToString()}")
                                       .Then(_ => _.SendAsync(channel, __ => __.ToConnectionStartOkFrame(connectionString.Credentials)))
                                       .LogError()
                                       .Log(_ => $"SENT: {_.ToString()}")
                                       .Then(_ => bag.For(ConnectionTune.StaticDescriptor)
                                                     .WaitForAsync<ConnectionTuneFrame>(_.Header.ChannelIndex))
                                       .LogError()
                                       .Log(_ => $"RECEIVED: {_.ToString()}")
                                       .Then(_ => _.SendAsync(channel, __ => __.ToConnectionTuneOkFrame()))
                                       .LogError()
                                       .Log(_ => $"SENT: {_.ToString()}")
                                       .Then(_ => _.SendAsync(channel, __ => __.ToConnectionOpenFrame(connectionString.VirtualHost)))
                                       .LogError()
                                       .Log(_ => $"SENT: {_.ToString()}")
                                       .Then(_ => bag.For(ConnectionOpenOk.StaticDescriptor)
                                                     .WaitForAsync<ConnectionOpenOkFrame>(_.Header.ChannelIndex))
                                       .LogError()
                                       .Log(_ => $"RECEIVED: {_.ToString()}")
                                       .Then(_ => new Connection(group, channel, bag, _.Header.ChannelIndex))
                                       .LogError();
        }

        public Task<IChannel> OpenChannelAsync()
        {
            var index = (Int16)Interlocked.Increment(ref channelIndex);

            return new ChannelOpenFrame(index,
                                        new ChannelOpen()).SendAsync(channel)
                                                          .LogError()
                                                          .Log(_ => $"SENT: {_.ToString()}")
                                                          .Then(_ => bag.For(ChannelOpenOk.StaticDescriptor)
                                                                                   .WaitForAsync<ChannelOpenOkFrame>(_.Header.ChannelIndex))
                                                          .LogError()
                                                          .Log(_ => $"RECEIVED: {_.ToString()}")
                                                          .ContinueWith<IChannel>(_ => new Channel(channel, bag, index));
        }

        public Task CloseAsync()
        {
            return ConnectionCloseFrame.Close()
                                       .SendAsync(channel)
                                       .LogError()
                                       .Log(_ => $"SENT: {_.ToString()}")
                                       .Then(_ => bag.For(ConnectionCloseOk.StaticDescriptor)
                                                     .WaitForAsync<ConnectionCloseOkFrame>(_.Header.ChannelIndex))
                                       .LogError()
                                       .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public void Dispose()
        {
            try
            {
                CloseAsync().LogError()
                            .Then(() => channel.CloseAsync())
                            .LogError()
                            .Then(() => group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100),
                                                                      TimeSpan.FromSeconds(1)))
                            .Unwrap()
                            .LogError()
                            .Wait(TimeSpan.FromSeconds(5d));
            }
            catch { }
        }
    }
}