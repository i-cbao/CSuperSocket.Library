using CSuperSocket.SocketBase.Command;
using CSuperSocket.SocketBase.Config;
using CSuperSocket.SocketBase.Protocol;
using System;

namespace CSuperSocket.SocketBase.Provider
{
    /// <summary>
    /// ProviderKey
    /// </summary>
    [Serializable]
    public class ProviderKey
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; private set; }

        private ProviderKey()
        {

        }

        static ProviderKey()
        {
            ServerType = new ProviderKey { Name = "ServerType", Type = typeof(IAppServer) };
            SocketServerFactory = new ProviderKey { Name = "SocketServerFactory", Type = typeof(ISocketServerFactory) };
            ConnectionFilter = new ProviderKey { Name = "ConnectionFilter", Type = typeof(IConnectionFilter) };
            ReceiveFilterFactory = new ProviderKey { Name = "ReceiveFilterFactory", Type = typeof(IReceiveFilterFactory) };
            CommandLoader = new ProviderKey { Name = "CommandLoader", Type = typeof(ICommandLoader) };
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        public static ProviderKey ServerType { get; private set; }

        /// <summary>
        /// Gets the socket server factory.
        /// </summary>
        public static ProviderKey SocketServerFactory { get; private set; }

        /// <summary>
        /// Gets the connection filter.
        /// </summary>
        public static ProviderKey ConnectionFilter { get; private set; }

     

        /// <summary>
        /// Gets the Receive filter factory.
        /// </summary>
        public static ProviderKey ReceiveFilterFactory { get; private set; }

        /// <summary>
        /// Gets the command loader.
        /// </summary>
        public static ProviderKey CommandLoader { get; private set; }
    }
}
