using System;
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
            const string exchangeName = "test-xchg";
            const ExchangeType exchangeType = ExchangeType.Direct;
            const bool isDurable = true;
            const bool isAutodelete = false;
            const bool isInternal = false;

            IConnection connection = null;
            IChannel channel = null;
            try
            {
                (connection, channel) = await BootstrapRabbit();
                await channel.ExchangeDeclareAsync(exchangeName, exchangeType, isDurable, isAutodelete, isInternal);
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }

            var retrievedExchange = rabbitMqManagementApi.GetExchange(exchangeName, Configuration.RabbitMqVirtualHost);

            Assert.NotNull(retrievedExchange);
            Assert.Equal(retrievedExchange.Name, exchangeName);
            Assert.Equal(retrievedExchange.Type, exchangeType.ToString(), StringComparer.OrdinalIgnoreCase);
            Assert.Equal(retrievedExchange.Durable, isDurable);
            Assert.Equal(retrievedExchange.AutoDelete, isAutodelete);
            Assert.Equal(retrievedExchange.Internal, isInternal);
        }

        [Fact]
        public async Task CreateQueue()
        {
            const string queueName = "test-queue";
            const bool isDurable = true;
            const bool isExclusive = false;
            const bool isAutoDelete = false;

            IConnection connection = null;
            IChannel channel = null;
            try
            {
                (connection, channel) = await BootstrapRabbit();
                await channel.QueueDeclareAsync(queueName, isDurable, isExclusive, isAutoDelete);
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }

            var retrievedQueue = rabbitMqManagementApi.GetQueue(queueName, Configuration.RabbitMqVirtualHost);

            Assert.NotNull(retrievedQueue);
            Assert.Equal(retrievedQueue.Name, queueName);
            Assert.Equal(retrievedQueue.Durable, isDurable);
            Assert.Equal(retrievedQueue.Exclusive, isExclusive);
            Assert.Equal(retrievedQueue.AutoDelete, isAutoDelete);
        }

        private Task<(IConnection connection, IChannel channel)> BootstrapRabbit()
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