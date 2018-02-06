using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Protocol;
using System.Net.Sockets;

namespace Dynamic.Net.WebSocket
{
    public interface IWebSocketClientSessionFactory
    {
        WebSocketSessionBase CreateSession(WebSocketClient client, ProtocolBase protocol, Socket clientSocket);
    }
}
