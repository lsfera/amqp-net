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
        private readonly string rabbitMqHost;
        private readonly int rabbitMqClientPort;
        private readonly int rabbitMqManagementPort;
        private readonly string rabbitMqVirtualHost;
        private readonly string rabbitMqUser;
        private readonly string rabbitMqPassword;

        private static Configuration Instance => LazyInstance.Value;

        public static string DockerHttpApiUri => Instance.dockerHttpApiUri;

        public static string RabbitMqHost => Instance.rabbitMqHost;

        public static int RabbitMqClientPort => Instance.rabbitMqClientPort;

        public static int RabbitMqManagementPort => Instance.rabbitMqManagementPort;

        public static string RabbitMqVirtualHost => Instance.rabbitMqVirtualHost;

        public static string RabbitMqUser => Instance.rabbitMqUser;

        public static string RabbitMqPassword => Instance.rabbitMqPassword;

        private Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json");
            var settings = builder.Build();

            dockerHttpApiUri = settings["dockerHttpApiUri"];
            rabbitMqHost = settings["rabbitMqHost"];
            rabbitMqClientPort = int.Parse(settings["rabbitMqClientPort"]);
            rabbitMqManagementPort = int.Parse(settings["rabbitMqManagementPort"]);
            rabbitMqVirtualHost = settings["rabbitMqVirtualHost"];
            rabbitMqUser = settings["rabbitMqUser"];
            rabbitMqPassword = settings["rabbitMqPassword"];
        }
    }
}