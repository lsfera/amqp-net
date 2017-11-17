using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amqp.Net.Client;
using EasyNetQ.Management.Client;
using EasyNetQ.Management.Client.Model;
using Xunit;
using Connection = Amqp.Net.Client.Connection;
using ExchangeType = Amqp.Net.Client.Entities.ExchangeType;

namespace Amqp.Net.Tests
{
    public class Integration : IClassFixture<RabbitMqFixture>
    {

        private readonly ManagementClient  rabbitMqManagementApi;

        public Integration()
        {

            rabbitMqManagementApi =  new ManagementClient(Configuration.RabbitMqHost, Configuration.RabbitMqUser, Configuration.RabbitMqPassword, Configuration.RabbitMqManagementPort);
        }

        [Fact]
        public async Task CreateExchange()
        {
            const string exchangeName = "test-xchg-create";
            const ExchangeType exchangeType = ExchangeType.Direct;
            const bool isDurable = true;
            const bool isAutodelete = false;
            const bool isInternal = false;

            IConnection connection = null;
            IChannel channel = null;
            try
            {
                (connection, channel) = await BootstrapRabbit().ConfigureAwait(false);
                await channel.ExchangeDeclareAsync(exchangeName, exchangeType, isDurable, isAutodelete, isInternal).ConfigureAwait(false);
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }

            var retrievedExchange = await rabbitMqManagementApi.GetExchangeAsync(exchangeName, Configuration.RabbitMqVirtualHost).ConfigureAwait(false);

            Assert.NotNull(retrievedExchange);
            Assert.Equal(retrievedExchange.Name, exchangeName);
            Assert.Equal(retrievedExchange.Type, exchangeType.ToString(), StringComparer.OrdinalIgnoreCase);
            Assert.Equal(retrievedExchange.Durable, isDurable);
            Assert.Equal(retrievedExchange.AutoDelete, isAutodelete);
            Assert.Equal(retrievedExchange.Internal, isInternal);
        }

        [Fact]
        public async Task DeleteExchange()
        {
            const string exchangeName = "test-xchg-delete";
            const ExchangeType exchangeType = ExchangeType.Direct;
            const bool isUnused = true;

            var exchange = rabbitMqManagementApi.CreateExchangeAsync(new ExchangeInfo(exchangeName, exchangeType.ToString().ToLower()), Configuration.RabbitMqVirtualHost);
            Assert.NotNull(exchange);

            IConnection connection = null;
            IChannel channel = null;
            try
            {
                (connection, channel) = await BootstrapRabbit().ConfigureAwait(false);
                await channel.ExchangeDeleteAsync(exchangeName, isUnused).ConfigureAwait(false);
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }
            try
            {
                await rabbitMqManagementApi.GetExchangeAsync(exchangeName, Configuration.RabbitMqVirtualHost).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Assert.IsType<UnexpectedHttpStatusCodeException>(e);
            }
            
        }

        [Fact]
        public async Task CreateQueue()
        {
            const string queueName = "test-queue-create";
            const bool isDurable = true;
            const bool isExclusive = false;
            const bool isAutoDelete = false;

            IConnection connection = null;
            IChannel channel = null;
            try
            {
                (connection, channel) = await BootstrapRabbit().ConfigureAwait(false);
                await channel.QueueDeclareAsync(queueName, isDurable, isExclusive, isAutoDelete).ConfigureAwait(false);
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }

            var retrievedQueue = await rabbitMqManagementApi.GetQueueAsync(queueName, Configuration.RabbitMqVirtualHost).ConfigureAwait(false);

            Assert.NotNull(retrievedQueue);
            Assert.Equal(retrievedQueue.Name, queueName);
            Assert.Equal(retrievedQueue.Durable, isDurable);
            Assert.Equal(retrievedQueue.Exclusive, isExclusive);
            Assert.Equal(retrievedQueue.AutoDelete, isAutoDelete);
        }

        [Fact]
        public async Task DeleteQueue()
        {
            const string queueName = "test-queue-delete";
            const bool ifUnused = true;
            const bool ifEmpty = false;

            IConnection connection = null;
            IChannel channel = null;
            try
            {
                (connection, channel) = await BootstrapRabbit().ConfigureAwait(false);
                await channel.QueueDeleteAsync(queueName, ifUnused, ifEmpty).ConfigureAwait(false);
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }
            try
            {
                await rabbitMqManagementApi.GetQueueAsync(queueName, Configuration.RabbitMqVirtualHost).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Assert.IsType<UnexpectedHttpStatusCodeException>(e);
            }

        }

        [Fact]
        public async Task QueueBind()
        {
            const bool isDurable = true;
            const string queueName = "test-queue-bind";
            const bool isExclusive = false;
            const bool isAutoDelete = false;

            const string exchangeName = "test-xchg-bind";
            const ExchangeType exchangeType = ExchangeType.Fanout;
            const bool isAutodelete = false;
            const bool isInternal = false;
            const string routingKey = "akey";

            IConnection connection = null;
            IChannel channel = null;
            try
            {
                (connection, channel) = await BootstrapRabbit().ConfigureAwait(false);
                await channel.ExchangeDeclareAsync(exchangeName, exchangeType, isDurable, isAutodelete, isInternal).ConfigureAwait(false);
                await channel.QueueDeclareAsync(queueName, isDurable, isExclusive, isAutoDelete).ConfigureAwait(false);
                await channel.QueueBindAsync(queueName, exchangeName, routingKey).ConfigureAwait(false);
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }

            var retrievedQueue = await rabbitMqManagementApi.GetQueueAsync(queueName, Configuration.RabbitMqVirtualHost).ConfigureAwait(false);
            var bindings = (await rabbitMqManagementApi.GetBindingsForQueueAsync(retrievedQueue).ConfigureAwait(false)).ToList();

            bool BindingFilter(Binding binding) => binding.Source == exchangeName 
                                            && binding.Destination == queueName 
                                            && binding.RoutingKey == routingKey;
            Assert.Contains(bindings, BindingFilter);
        }


        private static Task<(IConnection connection, IChannel channel)> BootstrapRabbit()
        {
            IConnection connection = null;
            IChannel channel = null;
            var ipAddress = IPAddress.Parse(Configuration.RabbitMqHost);
            var ipEndPoint = new IPEndPoint(ipAddress, Configuration.RabbitMqClientPort);
            var networkCredential = new NetworkCredential(Configuration.RabbitMqUser, Configuration.RabbitMqPassword);
            var connectionString = new ConnectionString(ipEndPoint, networkCredential, Configuration.RabbitMqVirtualHostName);

            Task<IChannel> OpenConnection(Task<IConnection> _)
            {
                connection = _.Result;
                return connection.OpenChannelAsync();
            }

            return Connection.ConnectAsync(connectionString)
                .ContinueWith(OpenConnection).Unwrap()
                .ContinueWith(__ =>
                {
                    channel = __.Result;
                    return (connection, channel);
                });

        }
    }
}