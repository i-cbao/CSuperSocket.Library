using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace CSuperSocket.SocketCK
{
    public static class SocketSetting
    {
        public static Socket GetStreamSocket(AddressFamily addressFamily)
        {
            var m_ListenSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            return m_ListenSocket;
        }
        public static void SetOp(Socket m_ListenSocket)
        {
            m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
               
            }
            else 
            {
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            }
        }
        public static void BdListen(Socket m_ListenSocket, IPEndPoint iPEndPoint, int backLog)
        {
            m_ListenSocket.Bind(iPEndPoint);
            m_ListenSocket.Listen(backLog);
        }
    }
}
