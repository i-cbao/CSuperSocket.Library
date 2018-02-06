using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using Dynamic.Net.KcpSharp.SeqCtrl;
using Dynamic.Core.Log;


namespace Dynamic.Net.KcpSharp.NetWork
{
    public class TcpNetworkDriver : SocketNetworkDriver
    {
        public TcpNetworkDriver(int port, bool isServer):base(port,isServer)
        {
            if (!UND.ContainsKey(port.ToString()))
            {
                 UND.Add(port.ToString(),this);
            }
        }

        protected override bool AsyReceive(SocketAsyncEventArgs saea)
        {
            return this.listenSocket.AcceptAsync(saea);
        }

        protected override Socket NewSocket(IPEndPoint localEndPoint)
        {
            try
            {
                int Backlog = this.numMaxConnections;
                var serverSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(localEndPoint);
                ///tcp才需要Udp不需要
                serverSocket.Listen(Backlog);

                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                return serverSocket;
            }
            catch (Exception e)
            {
                _Logger.Error("启动监听时发生异常：{0}", e.ToString());
            }
            return null;
        }
    }
}
