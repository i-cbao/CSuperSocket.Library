using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Dynamic.Net.Base;

namespace Dynamic.Net.Session
{
    public interface ISocketSessionFactory
    {
        INetSession CreateSession(INetApplication application, INetProtocol protocol, INetServer server, Socket client );
    }
}
