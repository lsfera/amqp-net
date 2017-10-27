using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Extensions;
using Amqp.Net.Client.Frames;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client
{
    public class Channel : IChannel
    {
        private readonly DotNetty.Transport.Channels.IChannel channel;
        private readonly IMethodFrameBag bag;
        private readonly Int16 channelIndex;

        internal Channel(DotNetty.Transport.Channels.IChannel channel,
                         IMethodFrameBag bag,
                         Int16 channelIndex)
        {
            this.channel = channel;
            this.bag = bag;
            this.channelIndex = channelIndex;
        }

        public void Dispose()
        {
            try
            {
                CloseAsync().LogError()
                            .Wait(TimeSpan.FromSeconds(5d));
            }
            catch { }
        }

        public Task CloseAsync()
        {
            return ChannelCloseFrame.Close(channelIndex)
                                    .SendAsync(channel)
                                    .LogError()
                                    .Log(_ => $"SENT: {_.ToString()}")
                                    .Then(_ => bag.For(ChannelCloseOk.StaticDescriptor)
                                                  .WaitForAsync<ChannelCloseOkFrame>(_.Header.ChannelIndex))
                                    .LogError()
                                    .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task ExchangeDeclareAsync(String name,
                                         ExchangeType type,
                                         Boolean durable,
                                         Boolean autoDelete,
                                         Boolean @internal)
        {
            var frame = new ExchangeDeclareFrame(channelIndex,
                                                 new ExchangeDeclare(0,
                                                                     name,
                                                                     type,
                                                                     false,
                                                                     durable,
                                                                     autoDelete,
                                                                     @internal,
                                                                     false,
                                                                     new Table(new Dictionary<String, Object>())));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(ExchangeDeclareOk.StaticDescriptor)
                                      .WaitForAsync<ExchangeDeclareOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task ExchangeBindAsync(String destination,
                                      String source,
                                      String routingKey)
        {
            // TODO: check the capability is available on server
            var frame = new ExchangeBindFrame(channelIndex,
                                              new ExchangeBind(0,
                                                               destination,
                                                               source,
                                                               routingKey,
                                                               false,
                                                               new Table(new Dictionary<String, Object>())));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(ExchangeBindOk.StaticDescriptor)
                                      .WaitForAsync<ExchangeBindOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task ExchangeUnbindAsync(String destination,
                                        String source,
                                        String routingKey)
        {
            // TODO: check the capability is available on server
            var frame = new ExchangeUnbindFrame(channelIndex,
                                                new ExchangeUnbind(0,
                                                                   destination,
                                                                   source,
                                                                   routingKey,
                                                                   false,
                                                                   new Table(new Dictionary<String, Object>())));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(ExchangeUnbindOk.StaticDescriptor)
                                      .WaitForAsync<ExchangeUnbindOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task ExchangeDeleteAsync(String name, Boolean ifUnused)
        {
            var frame = new ExchangeDeleteFrame(channelIndex,
                                                new ExchangeDelete(0,
                                                                   name,
                                                                   ifUnused,
                                                                   false));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(ExchangeDeleteOk.StaticDescriptor)
                                      .WaitForAsync<ExchangeDeleteOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task QueueDeclareAsync(String name,
                                      Boolean durable,
                                      Boolean exclusive,
                                      Boolean autoDelete)
        {
            var frame = new QueueDeclareFrame(channelIndex,
                                              new QueueDeclare(0,
                                                               name,
                                                               false,
                                                               durable,
                                                               exclusive,
                                                               autoDelete,
                                                               false,
                                                               new Table(new Dictionary<String, Object>())));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(QueueDeclareOk.StaticDescriptor)
                                      .WaitForAsync<QueueDeclareOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task QueueBindAsync(String queueName,
                                   String exchangeName,
                                   String routingKey)
        {
            var frame = new QueueBindFrame(channelIndex,
                                           new QueueBind(0,
                                                         queueName,
                                                         exchangeName,
                                                         routingKey,
                                                         false,
                                                         new Table(new Dictionary<String, Object>())));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(QueueBindOk.StaticDescriptor)
                                      .WaitForAsync<QueueBindOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task QueueUnbindAsync(String queueName,
                                     String exchangeName,
                                     String routingKey)
        {
            var frame = new QueueUnbindFrame(channelIndex,
                                             new QueueUnbind(0,
                                                             queueName,
                                                             exchangeName,
                                                             routingKey,
                                                             new Table(new Dictionary<String, Object>())));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(QueueUnbindOk.StaticDescriptor)
                                      .WaitForAsync<QueueUnbindOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task QueueDeleteAsync(String name,
                                     Boolean ifUnused,
                                     Boolean ifEmpty)
        {
            var frame = new QueueDeleteFrame(channelIndex,
                                             new QueueDelete(0,
                                                             name,
                                                             ifUnused,
                                                             ifEmpty,
                                                             false));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(QueueDeleteOk.StaticDescriptor)
                                      .WaitForAsync<QueueDeleteOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task BasicQosAsync(Int16 prefetchCount, Boolean global)
        {
            var frame = new BasicQosFrame(channelIndex,
                                          new BasicQos(0, // HACK: it seems RabbitMQ does not implement this: ({amqp_error,not_implemented,"prefetch_size!=0 (32768000)",'basic.qos'})
                                                       prefetchCount,
                                                       global));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(BasicQosOk.StaticDescriptor)
                                      .WaitForAsync<BasicQosOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }

        public Task BasicConsumeAsync(String queueName,
                                      String consumerTag,
                                      Boolean noLocal,
                                      Boolean noAck,
                                      Boolean exclusive)
        {
            var frame = new BasicConsumeFrame(channelIndex,
                                              new BasicConsume(0,
                                                               queueName,
                                                               consumerTag,
                                                               noLocal,
                                                               noAck,
                                                               exclusive,
                                                               false,
                                                               new Table(new Dictionary<String, Object>())));
            return frame.SendAsync(channel)
                        .LogError()
                        .Log(_ => $"SENT: {_.ToString()}")
                        .Then(_ => bag.For(BasicConsumeOk.StaticDescriptor)
                                      .WaitForAsync<BasicConsumeOkFrame>(_.Header.ChannelIndex))
                        .LogError()
                        .Log(_ => $"RECEIVED: {_.ToString()}");
        }
    }
}