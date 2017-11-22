using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EasyNetQ.Management.Client.Model;
using EasyNetQ.Management.Client.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyNetQ.Management.Client
{
    public class ManagementClient : IManagementClient
    {
        private static readonly MediaTypeWithQualityHeaderValue JsonMediaTypeHeaderValue =
            new MediaTypeWithQualityHeaderValue("application/json");

        public static readonly JsonSerializerSettings Settings;

        private readonly Action<HttpRequestMessage> configureRequest;
        private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(20);
        private readonly HttpClient httpClient;
        private readonly Regex urlRegex = new Regex(@"^(http|https):\/\/.+\w$", RegexOptions.Compiled|RegexOptions.Singleline);

        static ManagementClient()
        {
            Settings = new JsonSerializerSettings
            {
                ContractResolver = new RabbitContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            Settings.Converters.Add(new PropertyConverter());
            Settings.Converters.Add(new MessageStatsOrEmptyArrayConverter());
            Settings.Converters.Add(new QueueTotalsOrEmptyArrayConverter());
            Settings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
            Settings.Converters.Add(new HaParamsConverter());
        }

        public string HostUrl { get; }

        public string Username { get; }

        public int PortNumber { get; }

        public ManagementClient(
            string hostUrl,
            string username,
            string password,
            int portNumber = 15672,
            TimeSpan? timeout = null,
            Action<HttpRequestMessage> configureRequest = null,
            bool ssl = false)
        {
            if (string.IsNullOrEmpty(hostUrl))
            {
                throw new ArgumentException("hostUrl is null or empty");
            }

            if (hostUrl.StartsWith("https://"))
                ssl = true;

            if (ssl)
            {
                if (hostUrl.Contains("http://"))
                    throw new ArgumentException("hostUrl is illegal");
                hostUrl = hostUrl.Contains("https://") ? hostUrl : "https://" + hostUrl;
            }
            else
            {
                if (hostUrl.Contains("https://"))
                    throw new ArgumentException("hostUrl is illegal");
                hostUrl = hostUrl.Contains("http://") ? hostUrl : "http://" + hostUrl;
            }
            if (!urlRegex.IsMatch(hostUrl) || !Uri.TryCreate(hostUrl, UriKind.Absolute, out var urlUri))
            {
                throw new ArgumentException("hostUrl is illegal");
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("username is null or empty");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("password is null or empty");
            }
            if (configureRequest == null)
            {
                configureRequest = x => { };
            }
            HostUrl = hostUrl;
            Username = username;
            PortNumber = portNumber;
            this.configureRequest = configureRequest;


            httpClient = new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(username, password)
            })
            {
                Timeout = timeout ?? defaultTimeout
            };

            //default WebRequest.KeepAlive to false to resolve spurious 'the request was aborted: the request was canceled' exceptions
            httpClient.DefaultRequestHeaders.Add("Connection", "close");
        }

        public Task<Overview> GetOverviewAsync(GetLengthsCriteria lengthsCriteria = null,
            GetRatesCriteria ratesCriteria = null)
        {
            return GetAsync<Overview>("overview", lengthsCriteria, ratesCriteria);
        }

        public Task<IEnumerable<Node>> GetNodesAsync()
        {
            return GetAsync<IEnumerable<Node>>("nodes");
        }

        public Task<Definitions> GetDefinitionsAsync()
        {
            return GetAsync<Definitions>("definitions");
        }

        public Task<IEnumerable<Connection>> GetConnectionsAsync()
        {
            return GetAsync<IEnumerable<Connection>>("connections");
        }

        public async Task CloseConnectionAsync(Connection connection)
        {
            Ensure.ArgumentNotNull(connection, nameof(connection));
            await DeleteAsync(string.Format("connections/{0}", connection.Name)).ConfigureAwait(false);
        }

        public Task<IEnumerable<Channel>> GetChannelsAsync()
        {
            return GetAsync<IEnumerable<Channel>>("channels");
        }

        public Task<Channel> GetChannelAsync(string channelName, GetRatesCriteria ratesCriteria = null)
        {
            return GetAsync<Channel>($"channels/{channelName}", ratesCriteria);
        }

        public Task<IEnumerable<Exchange>> GetExchangesAsync()
        {
            return GetAsync<IEnumerable<Exchange>>("exchanges");
        }

        public Task<Exchange> GetExchangeAsync(string exchangeName, Vhost vhost, GetRatesCriteria ratesCriteria = null)
        {
            return GetAsync<Exchange>($"exchanges/{SanitiseVhostName(vhost.Name)}/{exchangeName}", ratesCriteria);
        }

        public Task<Queue> GetQueueAsync(string queueName, Vhost vhost, GetLengthsCriteria lengthsCriteria = null, GetRatesCriteria ratesCriteria = null)
        {
            return GetAsync<Queue>($"queues/{SanitiseVhostName(vhost.Name)}/{SanitiseName(queueName)}", lengthsCriteria, ratesCriteria);
        }

        public async Task<Exchange> CreateExchangeAsync(ExchangeInfo exchangeInfo, Vhost vhost)
        {
            if (exchangeInfo == null)
            {
                throw new ArgumentNullException(nameof(exchangeInfo));
            }
            if (vhost == null)
            {
                throw new ArgumentNullException(nameof(vhost));
            }

            await PutAsync($"exchanges/{SanitiseVhostName(vhost.Name)}/{SanitiseName(exchangeInfo.GetName())}", exchangeInfo).ConfigureAwait(false);

            return await GetExchangeAsync(SanitiseName(exchangeInfo.GetName()), vhost).ConfigureAwait(false);
        }

        public async Task DeleteExchangeAsync(Exchange exchange)
        {
            Ensure.ArgumentNotNull(exchange, nameof(exchange));

            await DeleteAsync($"exchanges/{SanitiseVhostName(exchange.Vhost)}/{SanitiseName(exchange.Name)}").ConfigureAwait(false);
        }

        public Task<IEnumerable<Binding>> GetBindingsWithSourceAsync(Exchange exchange)
        {
            Ensure.ArgumentNotNull(exchange, nameof(exchange));

            return GetAsync<IEnumerable<Binding>>($"exchanges/{SanitiseVhostName(exchange.Vhost)}/{exchange.Name}/bindings/source");
        }

        public Task<IEnumerable<Binding>> GetBindingsWithDestinationAsync(Exchange exchange)
        {
            Ensure.ArgumentNotNull(exchange, nameof(exchange));

            return GetAsync<IEnumerable<Binding>>($"exchanges/{SanitiseVhostName(exchange.Vhost)}/{exchange.Name}/bindings/destination");
        }

        public Task<PublishResult> PublishAsync(Exchange exchange, PublishInfo publishInfo)
        {
            Ensure.ArgumentNotNull(exchange, nameof(exchange));
            Ensure.ArgumentNotNull(publishInfo, nameof(publishInfo));

            return PostAsync<PublishInfo, PublishResult>($"exchanges/{SanitiseVhostName(exchange.Vhost)}/{exchange.Name}/publish", publishInfo);
        }

        public Task<IEnumerable<Queue>> GetQueuesAsync()
        {
            return GetAsync<IEnumerable<Queue>>("queues");
        }

        public async Task<Queue> CreateQueueAsync(QueueInfo queueInfo, Vhost vhost)
        {
            Ensure.ArgumentNotNull(queueInfo, nameof(queueInfo));
            Ensure.ArgumentNotNull(vhost, nameof(vhost));

            await PutAsync($"queues/{SanitiseVhostName(vhost.Name)}/{SanitiseName(queueInfo.GetName())}", queueInfo).ConfigureAwait(false);

            return await GetQueueAsync(queueInfo.GetName(), vhost).ConfigureAwait(false);
        }

        public async Task DeleteQueueAsync(Queue queue)
        {
            Ensure.ArgumentNotNull(queue, nameof(queue));

            await DeleteAsync(string.Format("queues/{0}/{1}", SanitiseVhostName(queue.Vhost), SanitiseName(queue.Name))).ConfigureAwait(false);
        }

        public Task<IEnumerable<Binding>> GetBindingsForQueueAsync(Queue queue)
        {
            Ensure.ArgumentNotNull(queue, nameof(queue));

            return GetAsync<IEnumerable<Binding>>(
                string.Format("queues/{0}/{1}/bindings", SanitiseVhostName(queue.Vhost), SanitiseName(queue.Name)));
        }

        public async Task PurgeAsync(Queue queue)
        {
            Ensure.ArgumentNotNull(queue, nameof(queue));

            await DeleteAsync(string.Format("queues/{0}/{1}/contents", SanitiseVhostName(queue.Vhost), SanitiseName(queue.Name))).ConfigureAwait(false);
        }

        public Task<IEnumerable<Message>> GetMessagesFromQueueAsync(Queue queue, GetMessagesCriteria criteria)
        {
            Ensure.ArgumentNotNull(queue, nameof(queue));

            return PostAsync<GetMessagesCriteria, IEnumerable<Message>>($"queues/{SanitiseVhostName(queue.Vhost)}/{SanitiseName(queue.Name)}/get", criteria);
        }

        public Task<IEnumerable<Binding>> GetBindingsAsync()
        {
            return GetAsync<IEnumerable<Binding>>("bindings");
        }

        public async Task CreateBinding(Exchange exchange, Queue queue, BindingInfo bindingInfo)
        {
            Ensure.ArgumentNotNull(exchange, nameof(exchange));
            Ensure.ArgumentNotNull(queue, nameof(queue));
            Ensure.ArgumentNotNull(bindingInfo, nameof(bindingInfo));

            await PostAsync<BindingInfo, object>(
                $"bindings/{SanitiseVhostName(queue.Vhost)}/e/{exchange.Name}/q/{SanitiseName(queue.Name)}", bindingInfo).ConfigureAwait(false);
        }

        public async Task CreateBinding(Exchange sourceExchange, Exchange destinationExchange, BindingInfo bindingInfo)
        {
            Ensure.ArgumentNotNull(sourceExchange, nameof(sourceExchange));
            Ensure.ArgumentNotNull(destinationExchange, nameof(destinationExchange));
            Ensure.ArgumentNotNull(bindingInfo, nameof(bindingInfo));

            await PostAsync<BindingInfo, object>(
                $"bindings/{SanitiseVhostName(sourceExchange.Vhost)}/e/{sourceExchange.Name}/e/{destinationExchange.Name}",
                bindingInfo).ConfigureAwait(false);
        }

        public Task<IEnumerable<Binding>> GetBindingsAsync(Exchange exchange, Queue queue)
        {
            Ensure.ArgumentNotNull(exchange, nameof(exchange));
            Ensure.ArgumentNotNull(queue, nameof(queue));

            return GetAsync<IEnumerable<Binding>>(
                $"bindings/{SanitiseVhostName(queue.Vhost)}/e/{exchange.Name}/q/{SanitiseName(queue.Name)}");
        }

        public Task<IEnumerable<Binding>> GetBindingsAsync(Exchange fromExchange, Exchange toExchange)
        {
            Ensure.ArgumentNotNull(fromExchange, nameof(fromExchange));
            Ensure.ArgumentNotNull(toExchange, nameof(toExchange));

            return GetAsync<IEnumerable<Binding>>(
                $"bindings/{SanitiseVhostName(toExchange.Vhost)}/e/{fromExchange.Name}/e/{SanitiseName(toExchange.Name)}");
        }

        public async Task DeleteBindingAsync(Binding binding)
        {
            Ensure.ArgumentNotNull(binding, nameof(binding));

            await DeleteAsync(
                $"bindings/{SanitiseVhostName(binding.Vhost)}/e/{binding.Source}/q/{binding.Destination}/{RecodeBindingPropertiesKey(binding.PropertiesKey)}").ConfigureAwait(false);
        }

        public Task<IEnumerable<Vhost>> GetVHostsAsync()
        {
            return GetAsync<IEnumerable<Vhost>>("vhosts");
        }

        public Task<Vhost> GetVhostAsync(string vhostName)
        {
            return GetAsync<Vhost>(string.Format("vhosts/{0}", SanitiseVhostName(vhostName)));
        }

        public async Task<Vhost> CreateVirtualHostAsync(string virtualHostName)
        {
            if (string.IsNullOrEmpty(virtualHostName))
            {
                throw new ArgumentException("virtualHostName is null or empty");
            }

            await PutAsync<String>($"vhosts/{virtualHostName}").ConfigureAwait(false);

            return await GetVhostAsync(virtualHostName).ConfigureAwait(false);
        }

        public async Task DeleteVirtualHostAsync(Vhost vhost)
        {
            Ensure.ArgumentNotNull(vhost, nameof(vhost));

            await DeleteAsync(string.Format("vhosts/{0}", vhost.Name)).ConfigureAwait(false);
        }

        public Task<IEnumerable<User>> GetUsersAsync()
        {
            return GetAsync<IEnumerable<User>>("users");
        }

        public Task<User> GetUserAsync(string userName)
        {
            return GetAsync<User>(string.Format("users/{0}", userName));
        }

        public Task<IEnumerable<Policy>> GetPoliciesAsync()
        {
            return GetAsync<IEnumerable<Policy>>("policies");
        }

        public async Task CreatePolicy(Policy policy)
        {
            Ensure.ArgumentNotNull(policy, nameof(policy));
            if (string.IsNullOrEmpty(policy.Name))
            {
                throw new ArgumentException("Policy name is empty");
            }
            if (string.IsNullOrEmpty(policy.Vhost))
            {
                throw new ArgumentException("vhost name is empty");
            }
            if (policy.Definition == null)
            {
                throw new ArgumentException("Definition should not be null");
            }

            await PutAsync(GetPolicyUrl(policy.Name, policy.Vhost), policy).ConfigureAwait(false);
        }

        public async Task DeletePolicyAsync(string policyName, Vhost vhost)
        {
            await DeleteAsync(GetPolicyUrl(policyName, vhost.Name)).ConfigureAwait(false);
        }

        public Task<IEnumerable<Parameter>> GetParametersAsync()
        {
            return GetAsync<IEnumerable<Parameter>>("parameters");
        }

        public async Task CreateParameterAsync(Parameter parameter)
        {
            var componentName = parameter.Component;
            var vhostName = parameter.Vhost;
            var parameterName = parameter.Name;
            await PutAsync(GetParameterUrl(componentName, vhostName, parameterName), parameter.Value).ConfigureAwait(false);
        }

        public async Task DeleteParameterAsync(string componentName, string vhost, string name)
        {
            await DeleteAsync(GetParameterUrl(componentName, vhost, name)).ConfigureAwait(false);
        }

        public async Task<User> CreateUserAsync(UserInfo userInfo)
        {
            Ensure.ArgumentNotNull(userInfo, nameof(userInfo));

            await PutAsync($"users/{userInfo.GetName()}", userInfo).ConfigureAwait(false);

            return await GetUserAsync(userInfo.GetName()).ConfigureAwait(false);
        }

        public async Task DeleteUserAsync(User user)
        {
            Ensure.ArgumentNotNull(user, nameof(user));

            await DeleteAsync($"users/{user.Name}").ConfigureAwait(false);
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return GetAsync<IEnumerable<Permission>>("permissions");
        }

        public async Task CreatePermissionAsync(PermissionInfo permissionInfo)
        {
            Ensure.ArgumentNotNull(permissionInfo, nameof(permissionInfo));

            await PutAsync($"permissions/{SanitiseVhostName(permissionInfo.GetVirtualHostName())}/{permissionInfo.GetUserName()}",
                permissionInfo).ConfigureAwait(false);
        }

        public async Task DeletePermissionAsync(Permission permission)
        {
            Ensure.ArgumentNotNull(permission, nameof(permission));

            await DeleteAsync($"permissions/{permission.Vhost}/{permission.User}").ConfigureAwait(false);
        }

        public async Task<User> ChangeUserPasswordAsync(string userName, string newPassword)
        {
            var user = await GetUserAsync(userName).ConfigureAwait(false);

            var tags = user.Tags.Split(',');
            var userInfo = new UserInfo(userName, newPassword);
            foreach (var tag in tags)
            {
                userInfo.AddTag(tag.Trim());
            }
            return await CreateUserAsync(userInfo).ConfigureAwait(false);
        }

        public Task<List<Federation>> GetFederationAsync()
        {
            return GetAsync<List<Federation>>("federation-links");
        }

        public async Task<bool> IsAliveAsync(Vhost vhost)
        {
            Ensure.ArgumentNotNull(vhost, nameof(vhost));

            var result = await GetAsync<AlivenessTestResult>($"aliveness-test/{SanitiseVhostName(vhost.Name)}").ConfigureAwait(false);
            return result.Status == "ok";
        }

        private Task<T> GetAsync<T>(string path, params object[] queryObjects)
        {
            var request = CreateRequestForPath(path, HttpMethod.Get, queryObjects);

            return httpClient.SendAsync(request)
                .ContinueWith(_ =>
                {
                    if (_.Exception != null)
                        throw(_.Exception.GetBaseException());

                    var response = _.Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new UnexpectedHttpStatusCodeException(response.StatusCode);
                    }
                    return DeserializeResponseAsync<T>(response);
                }).Unwrap();
        }

        private Task<TResult> PostAsync<TItem, TResult>(string path, TItem item)
        {
            var request = CreateRequestForPath(path, HttpMethod.Post);

            InsertRequestBody(request, item);

            return httpClient.SendAsync(request)
                .ContinueWith(_ =>
                {
                    if (_.Exception != null)
                        throw _.Exception.GetBaseException();

                    var response = _.Result;
                    if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created))
                        throw new UnexpectedHttpStatusCodeException(response.StatusCode);

                    var responseAsync = DeserializeResponseAsync<TResult>(response);
                    response.Dispose();
                    return responseAsync;
                }).Unwrap();
        }

        private async Task DeleteAsync(string path)
        {
            var request = CreateRequestForPath(path, HttpMethod.Delete);

            await httpClient.SendAsync(request)
                .ContinueWith(_ =>
                {
                    if (_.Exception != null)
                        throw _.Exception.GetBaseException();

                    var response = _.Result;
                    if (response.StatusCode != HttpStatusCode.NoContent)
                        throw new UnexpectedHttpStatusCodeException(response.StatusCode);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
        }

        private Task PutAsync<T>(string path, T item = default(T)) where T : class
        {
            var request = CreateRequestForPath(path, HttpMethod.Put);
            if (item != default(T))
                InsertRequestBody(request, item);

            return httpClient.SendAsync(request)
                .ContinueWith(_ =>
                {
                    if (_.Exception != null)
                        throw _.Exception.GetBaseException();

                    var response = _.Result;
                    if (!(response.StatusCode == HttpStatusCode.OK ||
                          response.StatusCode == HttpStatusCode.Created ||
                          response.StatusCode == HttpStatusCode.NoContent))
                    {
                        throw new UnexpectedHttpStatusCodeException(response.StatusCode);
                    }
                    return Task.CompletedTask;
                }).Unwrap();
        }

        private static void InsertRequestBody<T>(HttpRequestMessage request, T item)
        {
            if (!request.Headers.Accept.Contains(JsonMediaTypeHeaderValue))
                request.Headers.Accept.Add(JsonMediaTypeHeaderValue);

            var body = JsonConvert.SerializeObject(item, Settings);
            var content = new StringContent(body);

            content.Headers.ContentType = JsonMediaTypeHeaderValue;
            request.Content = content;
        }

        private static string GetPolicyUrl(string policyName, string vhost)
        {
            return $"policies/{SanitiseVhostName(vhost)}/{policyName}";
        }

        private static string GetParameterUrl(string componentName, string vhost, string parameterName)
        {
            return $"parameters/{componentName}/{SanitiseVhostName(vhost)}/{parameterName}";
        }

        private static Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            return GetBodyFromResponse(response)
                .ContinueWith(_ => JsonConvert.DeserializeObject<T>(_.Result, Settings));
        }

        private static Task<string> GetBodyFromResponse([NotNull] HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new EasyNetQManagementException("Response stream was null");
            }

            return response.Content.ReadAsStringAsync();
        }

        private HttpRequestMessage CreateRequestForPath(string path, HttpMethod httpMethod,
            IReadOnlyCollection<object> queryObjects = null)
        {
            var queryString = BuildQueryString(queryObjects);

            var uri = new Uri($"{HostUrl}:{PortNumber}/api/{path}{queryString}");
            var request = new HttpRequestMessage(httpMethod, uri);

            configureRequest(request);

            return request;
        }

        // Very simple query-string builder. 
        private static string BuildQueryString(IReadOnlyCollection<object> queryObjects)
        {
            if (queryObjects == null || queryObjects.Count == 0)
                return string.Empty;

            var queryStringBuilder = new StringBuilder("?");
            var first = true;
            // One or more query objects can be used to build the query
            foreach (var query in queryObjects)
            {
                if (query == null)
                    continue;
                // All public properties are added to the query on the format property_name=value
                var type = query.GetType();
                foreach (var prop in type.GetProperties())
                {
                    var name = Regex.Replace(prop.Name, "([a-z])([A-Z])", "$1_$2").ToLower();
                    var value = prop.GetValue(query, null);
                    if (!first)
                        queryStringBuilder.Append("&");
                    queryStringBuilder.AppendFormat("{0}={1}", name, value ?? string.Empty);
                    first = false;
                }
            }
            return queryStringBuilder.ToString();
        }

        private static string SanitiseVhostName(string vhostName)
        {
            return vhostName.Replace("/", "%2f");
        }

        private static string SanitiseName(string queueName)
        {
            return queueName.Replace("+", "%2B").Replace("#", "%23").Replace("/", "%2f");
        }

        private static string RecodeBindingPropertiesKey(string propertiesKey)
        {
            return propertiesKey.Replace("%5F", "%255F");
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
