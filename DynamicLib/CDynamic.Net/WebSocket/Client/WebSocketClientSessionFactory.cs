using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketClientSessionFactory : IWebSocketClientSessionFactory
    {
        #region IWebSocketClientSessionFactory 成员

        public WebSocketSessionBase CreateSession(WebSocketClient client, Dynamic.Net.Protocol.ProtocolBase protocol, System.Net.Sockets.Socket clientSocket)
        {
            return new WebSocketClientSession(client, protocol, clientSocket);
        }

        #endregion
    }
}
