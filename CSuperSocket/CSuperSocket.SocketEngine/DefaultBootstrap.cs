using CSuperSocket.Common;
using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Config;
using CSuperSocket.SocketBase.Metadata;
using Dynamic.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if !NETSTANDARD2_0
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
#endif
using System.Runtime.Serialization.Formatters;

namespace CSuperSocket.SocketEngine
{
    /// <summary>
    /// CSuperSocket default bootstrap
    /// </summary>
    public partial class DefaultBootstrap : IBootstrap, ILoggerProvider, IDisposable
    {
        private List<IWorkItem> m_AppServers;

        private IWorkItem m_ServerManager;

        /// <summary>
        /// Indicates whether the bootstrap is initialized
        /// </summary>
        private bool m_Initialized = false;

        /// <summary>
        /// Global configuration
        /// </summary>
        private IConfigurationSource m_Config;

        /// <summary>
        /// Global log
        /// </summary>
        private ILogger m_GlobalLog;

        /// <summary>
        /// Gets the bootstrap logger.
        /// </summary>
        ILogger ILoggerProvider.Logger
        {
            get { return m_GlobalLog; }
        }

    
      
        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        public IEnumerable<IWorkItem> AppServers
        {
            get { return m_AppServers; }
        }

        private readonly IRootConfig m_RootConfig;

        /// <summary>
        /// Gets the config.
        /// </summary>
        public IRootConfig Config
        {
            get
            {
                if (m_Config != null)
                    return m_Config;

                return m_RootConfig;
            }
        }

        /// <summary>
        /// Gets the startup config file.
        /// </summary>
        public string StartupConfigFile { get; private set; }

        /// <summary>
        /// Gets the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        public IPerformanceMonitor PerfMonitor { get { return m_PerfMonitor; } }

        private PerformanceMonitor m_PerfMonitor;

        private readonly string m_BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Gets the base directory.
        /// </summary>
        /// <value>
        /// The base directory.
        /// </value>
        public string BaseDirectory
        {
            get
            {
                return m_BaseDirectory;
            }
        }

        partial void SetDefaultCulture(IRootConfig rootConfig);

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        /// <param name="appServers">The app servers.</param>
        public DefaultBootstrap(IEnumerable<IWorkItem> appServers)
            : this(new RootConfig(), appServers)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="appServers">The app servers.</param>
        /// <param name="logFactory">The log factory.</param>
        public DefaultBootstrap(IRootConfig rootConfig, IEnumerable<IWorkItem> appServers)
        {
            if (rootConfig == null)
                throw new ArgumentNullException("rootConfig");

            if (appServers == null)
                throw new ArgumentNullException("appServers");

            if (!appServers.Any())
                throw new ArgumentException("appServers must have one item at least", "appServers");


            m_RootConfig = rootConfig;

            SetDefaultCulture(rootConfig);

            m_AppServers = appServers.ToList();

            m_GlobalLog = LoggerManager.GetLogger(this.GetType().Name);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (!rootConfig.DisablePerformanceDataCollector)
            {
                m_PerfMonitor = new PerformanceMonitor(rootConfig, m_AppServers, null);

               
                    m_GlobalLog.Debug("The PerformanceMonitor has been initialized!");
            }

                m_GlobalLog.Debug("The Bootstrap has been initialized!");

            m_Initialized = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public DefaultBootstrap(IConfigurationSource config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            SetDefaultCulture(config);

#if !NETSTANDARD2_0
            var fileConfigSource = config as ConfigurationSection;

            if (fileConfigSource != null)
                StartupConfigFile = fileConfigSource.GetConfigSource();
#else

#endif

            m_Config = config;

            AppDomain.CurrentDomain.SetData("Bootstrap", this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBootstrap"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="startupConfigFile">The startup config file.</param>
        public DefaultBootstrap(IConfigurationSource config, string startupConfigFile)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            SetDefaultCulture(config);

            if (!string.IsNullOrEmpty(startupConfigFile))
                StartupConfigFile = startupConfigFile;

            m_Config = config;

            AppDomain.CurrentDomain.SetData("Bootstrap", this);
        }

        /// <summary>
        /// Creates the work item instance.
        /// </summary>
        /// <param name="serviceTypeName">Name of the service type.</param>
        /// <param name="serverStatusMetadata">The server status metadata.</param>
        /// <returns></returns>
        protected virtual IWorkItem CreateWorkItemInstance(string serviceTypeName, StatusInfoAttribute[] serverStatusMetadata)
        {
            var serviceType = Type.GetType(serviceTypeName, true);
            return Activator.CreateInstance(serviceType) as IWorkItem;
        }

        internal virtual bool SetupWorkItemInstance(IWorkItem workItem, WorkItemFactoryInfo factoryInfo)
        {
            try
            {
                //Share AppDomain AppServers also share same socket server factory and log factory instances
                factoryInfo.SocketServerFactory.ExportFactory.EnsureInstance();
            }
            catch (Exception e)
            {
                m_GlobalLog.Error(e.ToString());
                return false;
            }

            return workItem.Setup(this, factoryInfo.Config, factoryInfo.ProviderFactories.ToArray());
        }

        /// <summary>
        /// Gets the work item factory info loader.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        internal virtual WorkItemFactoryInfoLoader GetWorkItemFactoryInfoLoader(IConfigurationSource config)
        {
            return new WorkItemFactoryInfoLoader(config);
        }

        /// <summary>
        /// Initializes the bootstrap with a listen endpoint replacement dictionary
        /// </summary>
        /// <param name="listenEndPointReplacement">The listen end point replacement.</param>
        /// <returns></returns>
        public virtual bool Initialize(IDictionary<string, IPEndPoint> listenEndPointReplacement)
        {
            return Initialize((c) => ReplaceListenEndPoint(c, listenEndPointReplacement));
        }

        private IServerConfig ReplaceListenEndPoint(IServerConfig serverConfig, IDictionary<string, IPEndPoint> listenEndPointReplacement)
        {
            var config = new ServerConfig(serverConfig);

            if (serverConfig.Port > 0)
            {
                var endPointKey = serverConfig.Name + "_" + serverConfig.Port;

                IPEndPoint instanceEndpoint;

                if (!listenEndPointReplacement.TryGetValue(endPointKey, out instanceEndpoint))
                {
                    throw new Exception(string.Format("Failed to find Input Endpoint configuration {0}!", endPointKey));
                }

                config.Ip = instanceEndpoint.Address.ToString();
                config.Port = instanceEndpoint.Port;
            }

            if (config.Listeners != null && config.Listeners.Any())
            {
                var listeners = config.Listeners.ToArray();

                for (var i = 0; i < listeners.Length; i++)
                {
                    var listener = (ListenerConfig)listeners[i];

                    var endPointKey = serverConfig.Name + "_" + listener.Port;

                    IPEndPoint instanceEndpoint;

                    if (!listenEndPointReplacement.TryGetValue(endPointKey, out instanceEndpoint))
                    {
                        throw new Exception(string.Format("Failed to find Input Endpoint configuration {0}!", endPointKey));
                    }

                    listener.Ip = instanceEndpoint.Address.ToString();
                    listener.Port = instanceEndpoint.Port;
                }

                config.Listeners = listeners;
            }

            return config;
        }

        private IWorkItem InitializeAndSetupWorkItem(WorkItemFactoryInfo factoryInfo)
        {
            IWorkItem appServer;

            try
            {
                appServer = CreateWorkItemInstance(factoryInfo.ServerType, factoryInfo.StatusInfoMetadata);

                    m_GlobalLog.Debug("The server instance {0} has been created!", factoryInfo.Config.Name);
            }
            catch (Exception e)
            {
                    m_GlobalLog.Error(string.Format("Failed to create server instance {0}!", factoryInfo.Config.Name), e);
                return null;
            }

            var exceptionSource = appServer as IExceptionSource;

            if (exceptionSource != null)
                exceptionSource.ExceptionThrown += new EventHandler<ErrorEventArgs>(exceptionSource_ExceptionThrown);


            var setupResult = false;

            try
            {
                setupResult = SetupWorkItemInstance(appServer, factoryInfo);

                    m_GlobalLog.Debug("The server instance {0} has been initialized!", appServer.Name);
            }
            catch (Exception e)
            {
                m_GlobalLog.Error(e.ToString());
                setupResult = false;
            }

            if (!setupResult)
            {
                    m_GlobalLog.Error("Failed to setup server instance!");

                return null;
            }

            return appServer;
        }


        /// <summary>
        /// Initializes the bootstrap with the configuration, config resolver and log factory.
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <returns></returns>
        public virtual bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            if (m_Initialized)
                throw new Exception("The server had been initialized already, you cannot initialize it again!");

            IEnumerable<WorkItemFactoryInfo> workItemFactories;

            using (var factoryInfoLoader = GetWorkItemFactoryInfoLoader(m_Config))
            {
          
                m_GlobalLog = LoggerManager.GetLogger(this.GetType().Name);

                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                try
                {
                    workItemFactories = factoryInfoLoader.LoadResult(serverConfigResolver);
                }
                catch (Exception e)
                {
                        m_GlobalLog.Error(e.ToString());

                    return false;
                }
            }

            m_AppServers = new List<IWorkItem>(m_Config.Servers.Count());

            IWorkItem serverManager = null;

            //Initialize servers
            foreach (var factoryInfo in workItemFactories)
            {
                IWorkItem appServer = InitializeAndSetupWorkItem(factoryInfo);

                if (appServer == null)
                    return false;

                if (factoryInfo.IsServerManager)
                    serverManager = appServer;
                else if (!(appServer is IsolationAppServer))//No isolation
                {
                    //In isolation mode, cannot check whether is server manager in the factory info loader
                    if (TypeValidator.IsServerManagerType(appServer.GetType()))
                        serverManager = appServer;
                }

                m_AppServers.Add(appServer);
            }

            if (serverManager != null)
                m_ServerManager = serverManager;

            if (!m_Config.DisablePerformanceDataCollector)
            {
                m_PerfMonitor = new PerformanceMonitor(m_Config, m_AppServers, serverManager);

                    m_GlobalLog.Debug("The PerformanceMonitor has been initialized!");
            }

                m_GlobalLog.Debug("The Bootstrap has been initialized!");

            try
            {
#if !NETSTANDARD2_0
                RegisterRemotingService();
#endif
            }
            catch (Exception e)
            {
                    m_GlobalLog.Error("Failed to register remoting access service!", e);

                return false;
            }

            m_Initialized = true;

            return true;
        }

        void exceptionSource_ExceptionThrown(object sender, ErrorEventArgs e)
        {
            m_GlobalLog.Error(string.Format("The server {0} threw an exception.", ((IWorkItemBase)sender).Name), e.Exception);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            m_GlobalLog.Error("The process crashed for an unhandled exception!", (Exception)e.ExceptionObject);
        }

      

      

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <returns></returns>
        public virtual bool Initialize()
        {
            return Initialize(c => c);
        }

        /// <summary>
        /// Starts this bootstrap.
        /// </summary>
        /// <returns></returns>
        public StartResult Start()
        {
            if (!m_Initialized)
            {
                    m_GlobalLog.Error("You cannot invoke method Start() before initializing!");

                return StartResult.Failed;
            }

            var result = StartResult.None;

            var succeeded = 0;

            foreach (var server in m_AppServers)
            {
                if (!server.Start())
                {
                        m_GlobalLog.Info("The server instance {0} has failed to be started!", server.Name);
                }
                else
                {
                    succeeded++;

                    if (Config.Isolation != IsolationMode.None)
                    {
                            m_GlobalLog.Info("The server instance {0} has been started!", server.Name);
                    }
                }
            }

            if (m_AppServers.Any())
            {
                if (m_AppServers.Count == succeeded)
                    result = StartResult.Success;
                else if (succeeded == 0)
                    result = StartResult.Failed;
                else
                    result = StartResult.PartialSuccess;
            }

            if (m_PerfMonitor != null)
            {
                m_PerfMonitor.Start();

                m_GlobalLog.Debug("The PerformanceMonitor has been started!");
            }

            return result;
        }

        /// <summary>
        /// Stops this bootstrap.
        /// </summary>
        public void Stop()
        {
            var servers = m_AppServers.ToArray();

            if (servers.Any(s => s.Config != null && s.Config.StartupOrder != 0))
            {
                Array.Reverse(servers);
            }

            foreach (var server in servers)
            {
                if (server.State == ServerState.Running)
                {
                    server.Stop();

                    if (Config.Isolation != IsolationMode.None)
                    {
                            m_GlobalLog.Info("The server instance {0} has been stopped!", server.Name);
                    }
                }
            }

            if (m_PerfMonitor != null)
            {
                m_PerfMonitor.Stop();

                    m_GlobalLog.Debug("The PerformanceMonitor has been stoppped!");
            }
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Registers the bootstrap remoting access service.
        /// </summary>
        protected virtual void RegisterRemotingService()
        {
            var bootstrapIpcPort = string.Format("CSuperSocket.Bootstrap[{0}]", Math.Abs(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar).GetHashCode()));

            var serverChannelName = "Bootstrap";

            var serverChannel = ChannelServices.RegisteredChannels.FirstOrDefault(c => c.ChannelName == serverChannelName);

            if (serverChannel != null)
                ChannelServices.UnregisterChannel(serverChannel);

            serverChannel = new IpcServerChannel(serverChannelName, bootstrapIpcPort, new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full });
            ChannelServices.RegisterChannel(serverChannel, false);

            AppDomain.CurrentDomain.SetData("BootstrapIpcPort", bootstrapIpcPort);

            var bootstrapProxyType = typeof(RemoteBootstrapProxy);

            if (!RemotingConfiguration.GetRegisteredWellKnownServiceTypes().Any(s => s.ObjectType == bootstrapProxyType))
                RemotingConfiguration.RegisterWellKnownServiceType(bootstrapProxyType, "Bootstrap.rem", WellKnownObjectMode.Singleton);
        }
#endif

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ResetPerfMoniter()
        {
            if (m_PerfMonitor != null)
            {
                m_PerfMonitor.Stop();
                m_PerfMonitor.Dispose();
                m_PerfMonitor = null;
            }

            m_PerfMonitor = new PerformanceMonitor(m_Config, m_AppServers, m_ServerManager);
            m_PerfMonitor.Start();

            m_GlobalLog.Debug("The PerformanceMonitor has been reset for new server has been added!");
        }
    }
}
