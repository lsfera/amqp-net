using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Amqp.Net.Tests
{
    public class DockerProxy : IDisposable
    {
        private readonly DockerClientConfiguration dockerConfiguration;
        private readonly DockerClient client;

        public DockerProxy(Uri uri)
        {
            dockerConfiguration = new DockerClientConfiguration(uri);
            client = dockerConfiguration.CreateClient();
        }

        public async Task<string> CreateNetworkAsync(string name, string driver = "bridge", CancellationToken token = default(CancellationToken))
        {
            var networksCreateParameters = new NetworksCreateParameters
            {
                Name = name,
                Driver = driver,
            };
            var response = await client.Networks.CreateNetworkAsync(networksCreateParameters, token);
            return response.ID;
        }

        public async Task<string> CreateContainerAsync(string image, string name, IDictionary<string, ISet<string>> portMappings, string networkName = null, IList<string> envVars = null, CancellationToken token = default(CancellationToken))
        {
            var createParameters = new CreateContainerParameters
            {
                Image = image,
                Env = envVars ?? Enumerable.Empty<string>().ToList(),
                Name = name,
                HostConfig = new HostConfig
                {
                    PortBindings = PortBindings(portMappings),
                    NetworkMode = networkName
                },
                ExposedPorts = portMappings.ToDictionary(x => x.Key, x => new EmptyStruct())
            };
            var response = await client.Containers.CreateContainerAsync(createParameters, token);
            return response.ID;
        }

        public async Task StartContainerAsync(string id, CancellationToken token = default(CancellationToken))
        {
            await client.Containers.StartContainerAsync(id, new ContainerStartParameters(), token);
        }

        public async Task StopContainerAsync(string name, CancellationToken token = default(CancellationToken))
        {
            var ids = await FindContainerIdsAsync(name);
            var stopTasks = ids.Select(x => client.Containers.StopContainerAsync(x, new ContainerStopParameters(), token));
            await (Task.WhenAll(stopTasks));
        }

        public async Task RemoveContainerAsync(string name, CancellationToken token = default(CancellationToken))
        {
            var ids = await FindContainerIdsAsync(name);
            var containerRemoveParameters = new ContainerRemoveParameters { Force = true, RemoveVolumes = true };
            var removeTasks = ids.Select(x => client.Containers.RemoveContainerAsync(x, containerRemoveParameters, token));
            await (Task.WhenAll(removeTasks));
        }

        public async Task DeleteNetworksAsync(string name, CancellationToken token = default(CancellationToken))
        {
            var ids = await FindNetworkIdsAsync(name);
            var deleteTasks = ids.Select(x => client.Networks.DeleteNetworkAsync(x, token));
            await (Task.WhenAll(deleteTasks));
        }

        public void Dispose()
        {
            dockerConfiguration.Dispose();
            client.Dispose();
        }

        private static IDictionary<string, IList<PortBinding>> PortBindings(IDictionary<string, ISet<string>> portMappings)
        {
            return portMappings
                .Select(x => new { ContainerPort = x.Key, HostPorts = HostPorts(x.Value) })
                .ToDictionary(x => x.ContainerPort, x => (IList<PortBinding>)x.HostPorts);
        }

        private static List<PortBinding> HostPorts(IEnumerable<string> hostPorts)
        {
            return hostPorts.Select(x => new PortBinding { HostPort = x }).ToList();
        }

        private async Task<IEnumerable<string>> FindContainerIdsAsync(string name)
        {
            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true, Filters = ListFilters(name) });
            return containers.Select(x => x.ID);
        }

        private async Task<IEnumerable<string>> FindNetworkIdsAsync(string name)
        {
            var networks = await client.Networks.ListNetworksAsync(new NetworksListParameters { Filters = ListFilters(name) });
            return networks.Select(x => x.ID);
        }

        private static Dictionary<string, IDictionary<string, bool>> ListFilters(string name)
        {
            return new Dictionary<string, IDictionary<string, bool>>
            {
                { "name", new Dictionary<string, bool>{ { name, true } } }
            };
        }
    }
}
