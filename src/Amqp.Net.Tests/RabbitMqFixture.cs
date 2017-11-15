using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common.Internal.Logging;
using RabbitMqHttpApiClient.API;
using Xunit;
using Xunit.Abstractions;

namespace Amqp.Net.Tests
{
    public class RabbitMqFixture : IAsyncLifetime, IDisposable
    {
        private const string DockerNetworkName = "bridgeWhaleNet";
        private const string RabbitImageName = "rabbitmq";
        private const string RabbitImageTag = "management";
        private const string RabbitContainerName = "rmq";
        private const int DefaultTimeoutSeconds = 20;

        private readonly DockerProxy dockerProxy;

        public RabbitMqFixture()
        {
            dockerProxy = new DockerProxy(new Uri(Configuration.DockerHttpApiUri));
        }

        public async Task InitializeAsync()
        {
            await DisposeAsync();

            await dockerProxy.CreateNetworkAsync(DockerNetworkName);

            await dockerProxy.PullImageAsync(RabbitImageName, RabbitImageTag);
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
            var containerId = await dockerProxy.CreateContainerAsync($"{RabbitImageName}:{RabbitImageTag}", RabbitContainerName, portMappings, DockerNetworkName, envVars);
            await dockerProxy.StartContainerAsync(containerId);
            await WaitForRabbitMqReady(new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeoutSeconds)).Token);
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

        private static async Task WaitForRabbitMqReady(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (await IsRabbitMqReady())
                    return;
                await Task.Delay(500, token);
            }
        }

        private static async Task<bool> IsRabbitMqReady()
        {
            var rabbitMqUrl = $"http://{Configuration.RabbitMqHost}:{Configuration.RabbitMqManagementPort}";
            var rabbitMqManagementApi = new RabbitMqApi(rabbitMqUrl, Configuration.RabbitMqUser, Configuration.RabbitMqPassword);

            try
            {
                return await rabbitMqManagementApi.GetDefinitionByVhost(Configuration.RabbitMqVirtualHost) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}