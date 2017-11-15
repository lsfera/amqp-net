using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amqp.Net.Client;
using Amqp.Net.Client.Entities;
using DotNetty.Common.Internal.Logging;
using Newtonsoft.Json.Linq;
using RabbitMqHttpApiClient.API;
using Xunit;
using Xunit.Abstractions;
using Connection = Amqp.Net.Client.Connection;

namespace Amqp.Net.Tests
{
    public class Integration : IAsyncLifetime, IDisposable
    {
        private const string DockerNetworkName = "bridgeWhaleNet";
        private const string RabbitImageName = "rabbitmq";
        private const string RabbitImageTag = "management";
        private const string RabbitContainerName = "rmq";
        private const int DefaultTimeoutSeconds = 10;

        private readonly ITestOutputHelper outputHelper;
        private readonly DockerProxy dockerProxy;

        public Integration(ITestOutputHelper testOutputHelper)
        {
            outputHelper = testOutputHelper;
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

        [Fact]
        public async Task CreateExchange()
        {
            InternalLoggerFactory.DefaultFactory.AddProvider(new TestOtputLoggerProvider(outputHelper));

            const string testXchg = "test-xchg";
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(Configuration.RabbitMqHost), Configuration.RabbitMqClientPort);
            var networkCredential = new NetworkCredential(Configuration.RabbitMqUser, Configuration.RabbitMqPassword);
            var connectionString = new ConnectionString(ipEndPoint, networkCredential, Configuration.RabbitMqVirtualHost);
            using (var connection = await Connection.ConnectAsync(connectionString))
            using (var channel = await connection.OpenChannelAsync())
            {
                await channel.ExchangeDeclareAsync(testXchg, ExchangeType.Direct, true, false, false);
            }

            var retrievedExchangeName = await new RabbitMqApi($"http://{Configuration.RabbitMqHost}:{Configuration.RabbitMqManagementPort}",
                Configuration.RabbitMqUser, Configuration.RabbitMqPassword).GetExchangeByVhostAndName(Configuration.RabbitMqVirtualHost, testXchg).ConfigureAwait(false);
            Assert.NotNull(retrievedExchangeName);
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

        private async Task WaitForRabbitMqReady(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (IsRabbitMqReady())
                    return;
                await Task.Delay(500, token);
            }
        }

        private bool IsRabbitMqReady()
        {
            var path = $"aliveness-test/{Configuration.RabbitMqVirtualHost.Replace("/", "%2f")}";
            var requestUri = new Uri($"http://{Configuration.RabbitMqHost}:{Configuration.RabbitMqManagementPort}/api/{path}");
            var webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            webRequest.Credentials = (ICredentials)new NetworkCredential(Configuration.RabbitMqUser, Configuration.RabbitMqPassword);

            try
            {
                var response = MakeRequest(webRequest);
                return JObject.Parse(response).SelectToken("status").Value<string>() == "ok";
            }
            catch
            {
                return false;
            }
        }

        private static string MakeRequest(WebRequest webRequest)
        {
            using (var httpResponse = (HttpWebResponse) webRequest.GetResponse())
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Status code {httpResponse.StatusCode}");

                using (var responseStream = httpResponse.GetResponseStream())
                using (var streamReader = new StreamReader(responseStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}