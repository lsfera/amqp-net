using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ.Management.Client.Model;
using JetBrains.Annotations;

namespace EasyNetQ.Management.Client
{
    public interface IManagementClient : IDisposable
    {
        /// <summary>
        /// The host URL that this instance is using.
        /// </summary>
        string HostUrl { get; }

        /// <summary>
        /// The Username that this instance is connecting as.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// The port number this instance connects using.
        /// </summary>
        int PortNumber { get; }

        /// <summary>
        /// Various random bits of information that describe the whole system.
        /// </summary>
        /// <param name="lengthsCriteria">Criteria for getting samples of queue length data</param>
        /// <param name="ratesCriteria">Criteria for getting samples of rate data</param>
        /// <returns></returns>
        Task<Overview> GetOverviewAsync(GetLengthsCriteria lengthsCriteria = null, GetRatesCriteria ratesCriteria = null);

        /// <summary>
        /// A list of nodes in the RabbitMQ cluster.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Node>> GetNodesAsync();

        /// <summary>
        /// The server definitions - exchanges, queues, bindings, users, virtual hosts, permissions. 
        /// Everything apart from messages.
        /// </summary>
        /// <returns></returns>
        Task<Definitions> GetDefinitionsAsync();

        /// <summary>
        /// A list of all open connections.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Connection>> GetConnectionsAsync();

        /// <summary>
        /// A list of all open channels.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Channel>> GetChannelsAsync();

        /// <summary>
        /// Gets the channel. This returns more detail, including consumers than the GetChannels method.
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="channelName">Channel name.</param>
        /// <param name="ratesCriteria">Criteria for getting samples of rate data</param>
        Task<Channel> GetChannelAsync (string channelName, GetRatesCriteria ratesCriteria = null);

        /// <summary>
        /// A list of all exchanges.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Exchange>> GetExchangesAsync();

        /// <summary>
        /// A list of all queues.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Queue>> GetQueuesAsync();

        /// <summary>
        /// A list of all bindings.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Binding>> GetBindingsAsync();

        /// <summary>
        /// A list of all vhosts.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Vhost>> GetVHostsAsync();

        /// <summary>
        /// A list of all users.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<User>> GetUsersAsync();

        /// <summary>
        /// A list of all permissions for all users.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Permission>> GetPermissionsAsync();

        /// <summary>
        /// Closes the given connection
        /// </summary>
        /// <param name="connection"></param>
        Task CloseConnectionAsync([NotNull] Connection connection);

        /// <summary>
        /// Creates the given exchange
        /// </summary>
        /// <param name="exchangeInfo"></param>
        /// <param name="vhost"></param>
        Task<Exchange> CreateExchangeAsync([NotNull] ExchangeInfo exchangeInfo, [NotNull] Vhost vhost);

        /// <summary>
        /// Delete the given exchange
        /// </summary>
        /// <param name="exchange"></param>
        Task DeleteExchangeAsync([NotNull] Exchange exchange);

        /// <summary>
        /// A list of all bindings in which a given exchange is the source.
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        Task<IEnumerable<Binding>> GetBindingsWithSourceAsync([NotNull] Exchange exchange);

        /// <summary>
        /// A list of all bindings in which a given exchange is the destination.
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        Task<IEnumerable<Binding>> GetBindingsWithDestinationAsync([NotNull] Exchange exchange);

        /// <summary>
        /// Publish a message to a given exchange.
        /// 
        /// Please note that the publish / get paths in the HTTP API are intended for injecting 
        /// test messages, diagnostics etc - they do not implement reliable delivery and so should 
        /// be treated as a sysadmin's tool rather than a general API for messaging.
        /// </summary>
        /// <param name="exchange">The exchange</param>
        /// <param name="publishInfo">The publication parameters</param>
        /// <returns>A PublishResult, routed == true if the message was sent to at least one queue</returns>
        Task<PublishResult> PublishAsync([NotNull] Exchange exchange, [NotNull] PublishInfo publishInfo);

        /// <summary>
        /// Create the given queue
        /// </summary>
        /// <param name="queueInfo"></param>
        /// <param name="vhost"></param>
        Task<Queue> CreateQueueAsync([NotNull] QueueInfo queueInfo, [NotNull] Vhost vhost);

        /// <summary>
        /// Delete the given queue
        /// </summary>
        /// <param name="queue"></param>
        Task DeleteQueueAsync([NotNull] Queue queue);

        /// <summary>
        /// A list of all bindings on a given queue.
        /// </summary>
        /// <param name="queue"></param>
        Task<IEnumerable<Binding>> GetBindingsForQueueAsync([NotNull] Queue queue);

        /// <summary>
        /// Purge a queue of all messages
        /// </summary>
        /// <param name="queue"></param>
        Task PurgeAsync([NotNull] Queue queue);

        /// <summary>
        /// Get messages from a queue.
        /// 
        /// Please note that the publish / get paths in the HTTP API are intended for 
        /// injecting test messages, diagnostics etc - they do not implement reliable 
        /// delivery and so should be treated as a sysadmin's tool rather than a 
        /// general API for messaging.
        /// </summary>
        /// <param name="queue">The queue to retrieve from</param>
        /// <param name="criteria">The criteria for the retrieve</param>
        /// <returns>Messages</returns>
        Task<IEnumerable<Message>> GetMessagesFromQueueAsync([NotNull] Queue queue, GetMessagesCriteria criteria);

        /// <summary>
        /// Create a binding between an exchange and a queue
        /// </summary>
        /// <param name="exchange">the exchange</param>
        /// <param name="queue">the queue</param>
        /// <param name="bindingInfo">properties of the binding</param>
        /// <returns>The binding that was created</returns>
        Task CreateBinding([NotNull] Exchange exchange, [NotNull] Queue queue, [NotNull] BindingInfo bindingInfo);

        /// <summary>
        /// Create a binding between an exchange and an exchange
        /// </summary>
        /// <param name="sourceExchange">the source exchange</param>
        /// <param name="destinationExchange">the destination exchange</param>
        /// <param name="bindingInfo">properties of the binding</param>
        Task CreateBinding([NotNull] Exchange sourceExchange, [NotNull] Exchange destinationExchange, [NotNull] BindingInfo bindingInfo);

        /// <summary>
        /// A list of all bindings between an exchange and a queue. 
        /// Remember, an exchange and a queue can be bound together many times!
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        Task<IEnumerable<Binding>> GetBindingsAsync([NotNull] Exchange exchange, [NotNull] Queue queue);

        /// <summary>
        /// A list of all bindings between an exchange and an exchange. 
        /// </summary>
        /// <param name="fromExchange"></param>
        /// <param name="toExchange"></param>
        /// <returns></returns>
        Task<IEnumerable<Binding>> GetBindingsAsync([NotNull] Exchange fromExchange, [NotNull] Exchange toExchange);

        /// <summary>
        /// Delete the given binding
        /// </summary>
        /// <param name="binding"></param>
        Task DeleteBindingAsync([NotNull] Binding binding);

        /// <summary>
        /// Create a new virtual host
        /// </summary>
        /// <param name="virtualHostName">The name of the new virtual host</param>
        Task<Vhost> CreateVirtualHostAsync(string virtualHostName);

        /// <summary>
        /// Delete a virtual host
        /// </summary>
        /// <param name="vhost">The virtual host to delete</param>
        Task DeleteVirtualHostAsync([NotNull] Vhost vhost);

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="userInfo">The user to create</param>
        Task<User> CreateUserAsync([NotNull] UserInfo userInfo);

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="user">The user to delete</param>
        Task DeleteUserAsync([NotNull] User user);

        /// <summary>
        /// Create a permission
        /// </summary>
        /// <param name="permissionInfo">The permission to create</param>
        Task CreatePermissionAsync([NotNull] PermissionInfo permissionInfo);

        /// <summary>
        /// Delete a permission
        /// </summary>
        /// <param name="permission">The permission to delete</param>
        Task DeletePermissionAsync([NotNull] Permission permission);

        /// <summary>
        /// Update the password of an user.
        /// </summary>
        /// <param name="userName">The name of a user</param>
        /// <param name="newPassword">The new password to set</param>
        Task<User> ChangeUserPasswordAsync(string userName, string newPassword);

        /// <summary>
        /// Declares a test queue, then publishes and consumes a message. Intended for use 
        /// by monitoring tools. If everything is working correctly, will return true.
        /// Note: the test queue will not be deleted (to to prevent queue churn if this 
        /// is repeatedly pinged).
        /// </summary>
        Task<bool> IsAliveAsync([NotNull] Vhost vhost);

        /// <summary>
        /// Get an individual exchange by name
        /// </summary>
        /// <param name="exchangeName">The name of the exchange</param>
        /// <param name="vhost">The virtual host that contains the exchange</param>
        /// <param name="ratesCriteria">Criteria for getting samples of rate data</param>
        /// <returns>The exchange</returns>
        Task<Exchange> GetExchangeAsync(string exchangeName, Vhost vhost, GetRatesCriteria ratesCriteria = null);

        /// <summary>
        /// Get an individual queue by name
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="vhost">The virtual host that contains the queue</param>
        /// <param name="lengthsCriteria">Criteria for getting samples of queue length data</param>
        /// <param name="ratesCriteria">Criteria for getting samples of rate data</param>
        /// <returns>The Queue</returns>
        Task<Queue> GetQueueAsync(string queueName, Vhost vhost, GetLengthsCriteria lengthsCriteria = null, GetRatesCriteria ratesCriteria = null);

        /// <summary>
        /// Get an individual vhost by name
        /// </summary>
        /// <param name="vhostName">The VHost</param>
        Task<Vhost> GetVhostAsync(string vhostName);

        /// <summary>
        /// Get a user by name
        /// </summary>
        /// <param name="userName">The name of the user</param>
        /// <returns>The User</returns>
        Task<User> GetUserAsync(string userName);

        /// <summary>
        /// Get collection of Policies on the cluster
        /// </summary>
        /// <returns>Policies</returns>
        Task<IEnumerable<Policy>> GetPoliciesAsync();

        /// <summary>
        /// Creates a policy on the cluster
        /// </summary>
        /// <param name="policy">Policy to create</param>
        Task CreatePolicy([NotNull] Policy policy);

        /// <summary>
        /// Delete a policy from the cluster
        /// </summary>
        /// <param name="policyName">Policy name</param>
        /// <param name="vhost">vhost on which the policy resides</param>
        Task DeletePolicyAsync(string policyName, Vhost vhost);

        /// <summary>
        /// Get all parameters on the cluster
        /// </summary>
        Task<IEnumerable<Parameter>> GetParametersAsync();

        /// <summary>
        /// Creates a parameter on the cluster
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="policy">Parameter to create</param>
        Task CreateParameterAsync(Parameter parameter);

        /// <summary>
        /// Delete a parameter from the cluster
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="vhost"></param>
        /// <param name="name"></param>
        Task DeleteParameterAsync(string componentName, string vhost, string name);

        /// <summary>
        /// Get list of federations
        /// </summary>
        /// <returns></returns>
        Task<List<Federation>> GetFederationAsync();
    }
}