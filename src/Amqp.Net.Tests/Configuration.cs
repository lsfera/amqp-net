using System;
using System.IO;
using EasyNetQ.Management.Client.Model;
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
        private readonly string rabbitMqVirtualHostName;
        private readonly Vhost rabbitMqVirtualHost;
        private readonly string rabbitMqUser;
        private readonly string rabbitMqPassword;

        private static Configuration Instance => LazyInstance.Value;

        public static string DockerHttpApiUri => Instance.dockerHttpApiUri;

        public static string RabbitMqHost => Instance.rabbitMqHost;

        public static int RabbitMqClientPort => Instance.rabbitMqClientPort;

        public static int RabbitMqManagementPort => Instance.rabbitMqManagementPort;

        public static string RabbitMqVirtualHostName => Instance.rabbitMqVirtualHostName;

        public static Vhost RabbitMqVirtualHost => Instance.rabbitMqVirtualHost;

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
            rabbitMqVirtualHostName = settings["rabbitMqVirtualHost"];
            rabbitMqVirtualHost = new Vhost { Name = rabbitMqVirtualHostName, Tracing = false };
            rabbitMqUser = settings["rabbitMqUser"];
            rabbitMqPassword = settings["rabbitMqPassword"];
        }
    }
}