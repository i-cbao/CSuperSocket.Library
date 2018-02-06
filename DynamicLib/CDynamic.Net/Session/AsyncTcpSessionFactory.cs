using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Server;

namespace Dynamic.Net.Session
{
    public class AsyncTcpSessionFactory : ISocketSessionFactory
    {
        #region ISocketSessionFactory 成员

        public Dynamic.Net.Base.INetSession CreateSession(Dynamic.Net.Base.INetApplication application, Dynamic.Net.Base.INetProtocol protocol, Dynamic.Net.Base.INetServer server, System.Net.Sockets.Socket client)
        {
            SocketSession s = new AsyncTcpSession(application, protocol, server, client);
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
