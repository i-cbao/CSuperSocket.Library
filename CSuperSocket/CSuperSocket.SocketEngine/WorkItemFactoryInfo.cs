using CSuperSocket.SocketBase.Config;
using CSuperSocket.SocketBase.Metadata;
using CSuperSocket.SocketBase.Provider;
using System.Collections.Generic;

namespace CSuperSocket.SocketEngine
{
    class WorkItemFactoryInfo
    {
        public string ServerType { get; set; }

        public bool IsServerManager { get; set; }

        public StatusInfoAttribute[] StatusInfoMetadata { get; set; }

        public IServerConfig Config { get; set; }

        public IEnumerable<ProviderFactoryInfo> ProviderFactories { get; set; }

        public ProviderFactoryInfo LogFactory { get; set; }

        public ProviderFactoryInfo SocketServerFactory { get; set; }
    }
}
