using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Config;
using CSuperSocket.SocketBase.Metadata;
using CSuperSocket.SocketEngine.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
#if !NETSTANDARD2_0
using System.Configuration;
using CSuperSocket.Common;
#else
using Microsoft.Extensions.Configuration;
#endif
using System.Linq;

namespace CSuperSocket.SocketEngine
{
    class AppDomainWorkItemFactoryInfoLoader : WorkItemFactoryInfoLoader
    {
        public AppDomainWorkItemFactoryInfoLoader(SocketBase.Config.IConfigurationSource config)
            : base(config)
        {
            InitliazeValidationAppDomain();
        }


        private AppDomain m_ValidationAppDomain;

        private TypeValidator m_Validator;

        private void InitliazeValidationAppDomain()
        {
#if !NETSTANDARD2_0
            m_ValidationAppDomain = AppDomain.CreateDomain("ValidationDomain", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.BaseDirectory, string.Empty, false);

            var validatorType = typeof(TypeValidator);
            m_Validator = (TypeValidator)m_ValidationAppDomain.CreateInstanceAndUnwrap(validatorType.Assembly.FullName, validatorType.FullName);
#else
            m_ValidationAppDomain = AppDomain.CreateDomain("ValidationDomain");
            var validatorType = typeof(TypeValidator);
            m_Validator = (TypeValidator)m_ValidationAppDomain.InitializeLifetimeService();
#endif
        }

        protected override string ValidateProviderType(string typeName)
        {
            if (!m_Validator.ValidateTypeName(typeName))
                throw new Exception(string.Format("Failed to load type {0}!", typeName));

            return typeName;
        }

        protected override ServerTypeMetadata GetServerTypeMetadata(string typeName)
        {
            return m_Validator.GetServerTypeMetadata(typeName);
        }

        public override void Dispose()
        {
            if (m_ValidationAppDomain != null)
            {
                AppDomain.Unload(m_ValidationAppDomain);
                m_ValidationAppDomain = null;
            }

            base.Dispose();
        }
    }

    class DefaultBootstrapAppDomainWrap : DefaultBootstrap
    {
        private IBootstrap m_Bootstrap;

        public DefaultBootstrapAppDomainWrap(IBootstrap bootstrap, SocketBase.Config.IConfigurationSource config, string startupConfigFile)
            : base(config, startupConfigFile)
        {
            m_Bootstrap = bootstrap;
        }

        protected override IWorkItem CreateWorkItemInstance(string serviceTypeName, StatusInfoAttribute[] serverStatusMetadata)
        {
            return new AppDomainAppServer(serviceTypeName, serverStatusMetadata);
        }

        internal override bool SetupWorkItemInstance(IWorkItem workItem, WorkItemFactoryInfo factoryInfo)
        {
            return workItem.Setup(m_Bootstrap, factoryInfo.Config, factoryInfo.ProviderFactories.ToArray());
        }

    }

    /// <summary>
    /// AppDomainBootstrap
    /// </summary>
    partial class AppDomainBootstrap : MarshalByRefObject, IBootstrap, IDisposable
    {
        private IBootstrap m_InnerBootstrap;

        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        public IEnumerable<IWorkItem> AppServers
        {
            get { return m_InnerBootstrap.AppServers; }
        }

        /// <summary>
        /// Gets the config.
        /// </summary>
        public IRootConfig Config
        {
            get { return m_InnerBootstrap.Config; }
        }
 
        /// <summary>
        /// Gets the startup config file.
        /// </summary>
        public string StartupConfigFile
        {
            get { return m_InnerBootstrap.StartupConfigFile; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainBootstrap"/> class.
        /// </summary>
        public AppDomainBootstrap(SocketBase.Config.IConfigurationSource config)
        {
            string startupConfigFile = string.Empty;

            if (config == null)
                throw new ArgumentNullException("config");

#if !NETSTANDARD2_0

            var configSectionSource = config as ConfigurationSection;

            if (configSectionSource != null)
                startupConfigFile = configSectionSource.GetConfigSource();

            //Keep serializable version of configuration
            if (!config.GetType().IsSerializable)
                config = new ConfigurationSource(config);
#else
            var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection()                          
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddXmlFile("App.config", optional: true, reloadOnChange: true)
            .Build();

            config = new ConfigurationSource(new SocketServiceConfig(configurationRoot));
#endif

            //Still use raw configuration type to bootstrap
            m_InnerBootstrap = CreateBootstrapWrap(this, config, startupConfigFile);

            AppDomain.CurrentDomain.SetData("Bootstrap", this);
        }

        protected virtual IBootstrap CreateBootstrapWrap(IBootstrap bootstrap, SocketBase.Config.IConfigurationSource config, string startupConfigFile)
        {
            return new DefaultBootstrapAppDomainWrap(this, config, startupConfigFile);
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            return m_InnerBootstrap.Initialize();
        }

        /// <summary>
        /// Initializes the bootstrap with the configuration and config resolver.
        /// </summary>
        /// <param name="serverConfigResolver">The server config resolver.</param>
        /// <returns></returns>
        public bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            return m_InnerBootstrap.Initialize(serverConfigResolver);
        }

    
        /// <summary>
        /// Initializes the bootstrap with a listen endpoint replacement dictionary
        /// </summary>
        /// <param name="listenEndPointReplacement">The listen end point replacement.</param>
        /// <returns></returns>
        public bool Initialize(IDictionary<string, System.Net.IPEndPoint> listenEndPointReplacement)
        {
            return m_InnerBootstrap.Initialize(listenEndPointReplacement);
        }

  
        /// <summary>
        /// Starts this bootstrap.
        /// </summary>
        /// <returns></returns>
        public StartResult Start()
        {
            return m_InnerBootstrap.Start();
        }

        /// <summary>
        /// Stops this bootstrap.
        /// </summary>
        public void Stop()
        {
            m_InnerBootstrap.Stop();
        }

        public string BaseDirectory
        {
            get { return m_InnerBootstrap.BaseDirectory; }
        }

        void IDisposable.Dispose()
        {
            var disposableBootstrap = m_InnerBootstrap as IDisposable;
            if (disposableBootstrap != null)
                disposableBootstrap.Dispose();
        }
    }
}
