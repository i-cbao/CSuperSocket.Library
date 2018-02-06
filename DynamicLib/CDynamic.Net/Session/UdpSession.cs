using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using System.Net.Sockets;

namespace Dynamic.Net.Session
{
    public abstract class UdpSession : SocketSession
    {


        public UdpSession(INetApplication application, INetProtocol protocol, INetServer server, Socket clientSocket)
            :base(application, protocol, server, clientSocket)
        {
        }

        public abstract void ReceivedData(byte[] receivedBytes);
    }
}
