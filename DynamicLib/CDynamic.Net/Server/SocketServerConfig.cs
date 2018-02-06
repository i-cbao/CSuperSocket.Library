using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using Dynamic.Net.Session;

namespace Dynamic.Net.Server
{
    [Serializable]
    public class SocketServerConfig : INetServerConfig
    {
        #region INetServerConfig 成员

        public string Name
        {
            get;
            set;
        }

        public NetServerType ServerType
        {
            get;
            set;
        }

        public System.Net.Sockets.AddressFamily AddressFamily
        {
            get;
            set;
        }

        public System.Net.IPAddress Address
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public int Backlog
        {
            get;
            set;
        }

        public int MaxConnectionNumber
        {
            get;
            set;
        }

        public SessionTimeoutType TimeoutType
        {
            get;
            set;
        }

        public long SessionTimeout
        {
            get;
            set;
        }
        #endregion
    }
}
