#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif
using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Config;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CSuperSocket.SocketEngine.Configuration
{
#if NETSTANDARD2_0
    public partial class SocketServiceConfig : ConfigurationRoot, CSuperSocket.SocketBase.Config.IConfigurationSource
    {
        /// <summary>
        /// Gets the max working threads.
        /// </summary>      
        public int MaxWorkingThreads { get; set; } = -1;

        /// <summary>
        /// Gets the min working threads.
        /// </summary>      
        public int MinWorkingThreads { get; set; } = -1;

        /// <summary>
        /// Gets the max completion port threads.
        /// </summary>     
        public int MaxCompletionPortThreads { get; set; } = -1;

        /// <summary>
        /// Gets the min completion port threads.
        /// </summary>        
        public int MinCompletionPortThreads { get; set; } = -1;

        /// <summary>
        /// Gets the performance data collect interval, in seconds.
        /// </summary>      
        public int PerformanceDataCollectInterval { get; set; } = 60;

        /// <summary>
        /// Gets a value indicating whether [disable performance data collector].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable performance data collector]; otherwise, <c>false</c>.
        /// </value>      
        public bool DisablePerformanceDataCollector { get; set; }

        /// <summary>
        /// Gets the isolation mode.
        /// </summary>      
        public IsolationMode Isolation { get; set; } = IsolationMode.None;

        /// <summary>
        /// Gets the logfactory name of the bootstrap.
        /// </summary>      
        public string LogFactory { get; set; }


        public List<Server> Servers { get; set; }

        public List<TypeProvider> ServerTypes { get; set; }

        public List<TypeProvider> ConnectionFilters { get; set; }

        public List<TypeProvider> LogFactories { get; set; }

        public List<TypeProvider> ReceiveFilterFactories { get; set; }

        public List<TypeProvider> CommandLoaders { get; set; }

        /// <summary>
        /// Gets the option elements.
        /// </summary>
        public NameValueCollection OptionElements { get; private set; }

        /// <summary>
        /// Gets/sets the default culture for all server instances.
        /// </summary>
        /// <value>
        /// The default culture.
        /// </value>     
        public string DefaultCulture { get; set; }


        public SocketServiceConfig(IConfiguration configuration)
            : base(new List<IConfigurationProvider>())
        {
            Servers = configuration.GetSection("CSuperSocket:servers:server").Get<List<Server>>();
            ServerTypes = configuration.GetSection("CSuperSocket:serverTypes:add").Get<List<TypeProvider>>();
            ConnectionFilters = configuration.GetSection("CSuperSocket:connectionfilters:add").Get<List<TypeProvider>>();
            LogFactories = configuration.GetSection("CSuperSocket:logfactories:add").Get<List<TypeProvider>>();
            ReceiveFilterFactories = configuration.GetSection("CSuperSocket:receivefilterfactories:add").Get<List<TypeProvider>>();
            CommandLoaders = configuration.GetSection("CSuperSocket:commandLoaders:add").Get<List<TypeProvider>>();
        }
        public SocketServiceConfig(List<Server> servers, List<TypeProvider> serverTypes=null, List<TypeProvider> connectionFilters=null, List<TypeProvider> logFactories=null, List<TypeProvider> receiveFilterFactories=null, List<TypeProvider> commandLoaders=null)
          : base(new List<IConfigurationProvider>())
        {
            Servers = servers;
            ServerTypes = serverTypes;
            ConnectionFilters = connectionFilters;
            LogFactories = logFactories;
            ReceiveFilterFactories = receiveFilterFactories;
            CommandLoaders = commandLoaders;
        }
        public SocketServiceConfig(Server server):base(new List<IConfigurationProvider>())
        {
            List<Server> serverList = new List<Server>();
            serverList.Add(server);
            this.Servers = serverList;
        }

        public TConfig GetChildConfig<TConfig>(string childConfigName) where TConfig : class, new()
        {
            return default(TConfig);
        }
    }
#endif
}
