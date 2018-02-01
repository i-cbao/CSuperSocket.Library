using CSuperSocket.SocketBase;
using CSuperSocket.SocketEngine.AsyncSocket;
using System.Net.Sockets;

namespace CSuperSocket.SocketEngine
{
    interface IAsyncSocketSessionBase : ILoggerProvider
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; }

        Socket Client { get; }
    }

    interface IAsyncSocketSession : IAsyncSocketSessionBase
    {
        void ProcessReceive(SocketAsyncEventArgs e);
    }
}
