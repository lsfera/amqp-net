using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amqp.Net.Client;
using Amqp.Net.Client.Entities;
using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Amqp.Net.Tests
{
    public class Integration : IAsyncLifetime, IDisposable
    {
        private const string DockerNetworkName = "bridgeWhaleNet";
        private const string RabbitContainerName = "rmq";

        private readonly ITestOutputHelper outputHelper;
        private readonly DockerProxy dockerProxy;
        private string networkId;
        private string containerId;

        public Integration(ITestOutputHelper testOutputHelper)
        {
            outputHelper = testOutputHelper;
            dockerProxy = new DockerProxy(new Uri(Configuration.DockerHttpApiUri));
        }

        public async Task InitializeAsync()
        {
            await DisposeAsync();

            networkId = await dockerProxy.CreateNetworkAsync(DockerNetworkName);

            var portMappings = new Dictionary<string, ISet<string>>
            {
                { "4369", new HashSet<string>(){ "4369" } },
                { "5671", new HashSet<string>(){ "5671" } },
                { "5672", new HashSet<string>(){ "5672" } } ,
                { "15671",new HashSet<string>(){ "15671" } },
                { "15672",new HashSet<string>(){ "15672" } },
                { "25672",new HashSet<string>(){ "25672" } }
            };
            var envVars = new List<string> { "RABBITMQ_DEFAULT_VHOST=test" };
            containerId = await dockerProxy.CreateContainerAsync("rabbitmq:management", RabbitContainerName, portMappings, DockerNetworkName, envVars);
            await dockerProxy.StartContainerAsync(containerId);
        }

        [Fact]
        public void Dummy()
        {
            Assert.True(true);
        }

        public async Task DisposeAsync()
        {
            await dockerProxy.StopContainerAsync(RabbitContainerName);
            await dockerProxy.RemoveContainerAsync(RabbitContainerName);
            await dockerProxy.DeleteNetworksAsync(DockerNetworkName);
        }

        public void Dispose()
        {
            dockerProxy.Dispose();
        }
    }
}