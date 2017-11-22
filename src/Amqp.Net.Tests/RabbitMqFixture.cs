using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Management.Client;
using Xunit;

namespace Amqp.Net.Tests
{
    public class RabbitMqFixture : IAsyncLifetime, IDisposable
    {
        private const string DockerNetworkName = "bridgeWhaleNet";
        private const string RabbitImageName = "rabbitmq";
        private const string RabbitImageTag = "management-alpine";
        private const string RabbitContainerName = "rmq";
        private const int DefaultTimeoutSeconds = 20;

        private readonly DockerProxy dockerProxy;

        public RabbitMqFixture()
        {
            dockerProxy = new DockerProxy(new Uri(Configuration.DockerHttpApiUri));
        }

        public async Task InitializeAsync()
        {
            await DisposeAsync().ConfigureAwait(false);

            await dockerProxy.CreateNetworkAsync(DockerNetworkName).ConfigureAwait(false);

            await dockerProxy.PullImageAsync(RabbitImageName, RabbitImageTag).ConfigureAwait(false);
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
            var containerId = await dockerProxy.CreateContainerAsync($"{RabbitImageName}:{RabbitImageTag}", RabbitContainerName, portMappings, DockerNetworkName, envVars).ConfigureAwait(false);
            await dockerProxy.StartContainerAsync(containerId).ConfigureAwait(false);
            await WaitForRabbitMqReady(new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeoutSeconds)).Token).ConfigureAwait(false);
        }

        public async Task DisposeAsync()
        {
            await dockerProxy.StopContainerAsync(RabbitContainerName).ConfigureAwait(false);
            await dockerProxy.RemoveContainerAsync(RabbitContainerName).ConfigureAwait(false);
            await dockerProxy.DeleteNetworksAsync(DockerNetworkName).ConfigureAwait(false);
        }

        public void Dispose()
        {
            dockerProxy.Dispose();
        }

        private static async Task WaitForRabbitMqReady(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (await IsRabbitMqReady())
                    return;
                await Task.Delay(500, token).ConfigureAwait(false);
            }
        }

        private static async Task<bool> IsRabbitMqReady()
        {
            var rabbitMqManagementApi = new ManagementClient(Configuration.RabbitMqHost, Configuration.RabbitMqUser, Configuration.RabbitMqPassword, Configuration.RabbitMqManagementPort);

            try
            {
                return await rabbitMqManagementApi.IsAliveAsync(Configuration.RabbitMqVirtualHost).ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
        }
    }
}