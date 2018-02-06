using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net
{
    public class DefaultUdpSessionFactory : IUdpSessionFactory
    {
        public bool IsServer { get; private set; }
        public DefaultUdpSessionFactory(bool isServer)
        {
            IsServer = isServer;
        }



        #region IUdpSessionFactory 成员

        public UdpSession CreateSession(UdpSocket client, DnsEndPoint target)
        {
            if (IsServer)
            {
                return new UdpServerSession() { Client = client as UdpServerSocketProxy, Target = target };
            }
            else
            {
                return new UdpSession() { Client = client, Target = target };
            }
        }

        #endregion
    }
}
