using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using Dynamic.Net.Server;

namespace Dynamic.Net.Session
{
    public class  SyncTcpSessionFactory : ISocketSessionFactory
    {
        #region ISocketSessionFactory 成员

        public INetSession CreateSession(INetApplication application, INetProtocol protocol, INetServer server, System.Net.Sockets.Socket client)
        {
            SocketSession s = new SocketSession(application, protocol, server, client);
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

        #endregion
    }
}
