using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using Dynamic.Net.Session;
using Dynamic.Net.Server;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketSessionFactory : ISocketSessionFactory
    {
        public INetSession CreateSession(INetApplication application, INetProtocol protocol, INetServer server, System.Net.Sockets.Socket client)
        {
            SocketSession s = new WebSocketSession(application, protocol, server, client);
            SocketServerConfig config = null;
            if (server is SocketServerBase)
            {
                config = (server as SocketServerBase).Config;
            }
            if (config != null)
            {
                s.Timeout = TimeSpan.FromMinutes(config.SessionTimeout);
                s.TimeoutType = config.TimeoutType;
            }

            return s;
        }
    }
}
