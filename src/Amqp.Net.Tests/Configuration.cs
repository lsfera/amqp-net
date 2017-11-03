using System;
using System.IO;
using Amqp.Net.Client;
using Microsoft.Extensions.Configuration;

namespace Amqp.Net.Tests
{
    internal sealed class Configuration
    {
        private static readonly Lazy<Configuration> LazyInstance = new Lazy<Configuration>(() => new Configuration());

        private readonly string dockerHttpApiUri;
        private readonly string rabbitMqConnectionString;

        private static Configuration Instance => LazyInstance.Value;

        public static string DockerHttpApiUri => Instance.dockerHttpApiUri;

        public static string RabbitMqConnectionString => Instance.rabbitMqConnectionString;

        private Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json");
            var settings = builder.Build();
            dockerHttpApiUri = settings["dockerHttpApiUri"];
            rabbitMqConnectionString = settings["rabbitMQConnectionString"];
        }
    }
}