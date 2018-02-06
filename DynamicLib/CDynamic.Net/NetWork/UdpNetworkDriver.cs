using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using Dynamic.Net.KcpSharp.SeqCtrl;


namespace Dynamic.Net.KcpSharp.NetWork
{
    public class UdpNetworkDriver : SocketNetworkDriver
    {
        public UdpNetworkDriver(int port, bool isServer):base(port,isServer)
        {
           
        }
        protected override bool AsyReceive(SocketAsyncEventArgs saea)
        {
            return this.listenSocket.ReceiveFromAsync(saea);
        }
        protected override Socket NewSocket(IPEndPoint localEndPoint)
        {
            var serverSocket=new Socket(localEndPoint.AddressFamily, SocketType.Dgram,
               ProtocolType.Udp);
            serverSocket.Bind(localEndPoint);
            return serverSocket;
        }
    }
}
