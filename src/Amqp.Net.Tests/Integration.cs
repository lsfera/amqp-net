using System;
using System.Net;
using System.Threading.Tasks;
using Amqp.Net.Client;
using Amqp.Net.Client.Entities;
using DotNetty.Common.Internal.Logging;
using RabbitMqHttpApiClient.API;
using Xunit;
using Xunit.Abstractions;
using Connection = Amqp.Net.Client.Connection;

namespace Amqp.Net.Tests
{
    public class Integration : IClassFixture<RabbitMqFixture>
    {
        private readonly RabbitMqApi rabbitMqManagementApi;

        public Integration(ITestOutputHelper testOutputHelper)
        {
            var rabbitMqUrl = $"http://{Configuration.RabbitMqHost}:{Configuration.RabbitMqManagementPort}";
            rabbitMqManagementApi = rabbitMqManagementApi = new RabbitMqApi(rabbitMqUrl, Configuration.RabbitMqUser, Configuration.RabbitMqPassword);
            //InternalLoggerFactory.DefaultFactory.AddProvider(new TestOtputLoggerProvider(testOutputHelper));
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

            var retrievedExchange = await rabbitMqManagementApi.GetExchangeByVhostAndName(Configuration.RabbitMqVirtualHost, exchangeName);

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

            var retrievedQueue = await rabbitMqManagementApi.GetQueueByVhostAndName(Configuration.RabbitMqVirtualHost, queueName);

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
            var connectionString = new ConnectionString(ipEndPoint, networkCredential, Configuration.RabbitMqVirtualHost);

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