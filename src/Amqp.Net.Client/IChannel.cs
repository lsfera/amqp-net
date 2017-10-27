using System;
using System.Threading.Tasks;
using Amqp.Net.Client.Entities;

namespace Amqp.Net.Client
{
    public interface IChannel : IDisposable
    {
        Task CloseAsync();

        Task ExchangeDeclareAsync(String name,
                                  ExchangeType type,
                                  Boolean durable,
                                  Boolean autoDelete,
                                  Boolean @internal);

        Task ExchangeDeleteAsync(String name, Boolean ifUnused);

        Task ExchangeBindAsync(String destination,
                               String source,
                               String routingKey);

        Task ExchangeUnbindAsync(String destination,
                                 String source,
                                 String routingKey);

        Task QueueDeclareAsync(String name,
                               Boolean durable,
                               Boolean exclusive,
                               Boolean autoDelete);

        Task QueueBindAsync(String queueName,
                            String exchangeName,
                            String routingKey);

        Task QueueUnbindAsync(String queueName,
                              String exchangeName,
                              String routingKey);

        Task QueueDeleteAsync(String name,
                              Boolean ifUnused,
                              Boolean ifEmpty);

        Task BasicQosAsync(Int16 prefetchCount, Boolean global);
    }
}