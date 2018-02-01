using CSuperSocket.SocketBase.Metadata;
using System;

namespace CSuperSocket.SocketEngine
{
    [Serializable]
    class ServerTypeMetadata
    {
        public StatusInfoAttribute[] StatusInfoMetadata { get; set; }

        public bool IsServerManager { get; set; }
    }
}
