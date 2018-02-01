using CSuperSocket.SocketBase.Config;
using System.Collections.Generic;

namespace CSuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Server configuration
    /// </summary>
    public partial class Server : IServerConfig
    {
        public PlatformType PlatformType { get; set; }
        /// <summary>
        /// Gets X509Certificate configuration.
        /// </summary>
        /// <value>
        /// X509Certificate configuration.
        /// </value>
        public ICertificateConfig Certificate
        {
            get { return CertificateConfig; }
        }
               
        /// <summary>
        /// Gets the listeners' configuration.
        /// </summary>
        IEnumerable<IListenerConfig> IServerConfig.Listeners
        {
            get
            {
                return this.Listeners;
            }
        }
      
        IEnumerable<ICommandAssemblyConfig> IServerConfig.CommandAssemblies
        {
            get { return this.CommandAssemblies; }
        }

        
    }
}
