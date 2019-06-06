using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Config;
using System;
using System.Net;
using System.Net.Sockets;

namespace CSuperSocket.SocketEngine
{
    abstract class SocketListenerBase : ISocketListener
    {
        public ListenerInfo Info { get; private set; }

        public abstract SocketMode CurrentSocketModel { get; }
        public IPEndPoint EndPoint
        {
            get { return Info.EndPoint; }
        }

        protected SocketListenerBase(ListenerInfo info)
        {
            Info = info;
        }

        /// <summary>
        /// Starts to listen
        /// </summary>
        /// <param name="config">The server config.</param>
        /// <returns></returns>
        public abstract bool Start(IServerConfig config);

        public abstract void Stop();

        public event NewClientAcceptHandler NewClientAccepted;

        public event ErrorHandler Error;

        protected void OnError(Exception e)
        {
            var handler = Error;

            if (handler != null)
                handler(this, e);
        }

        protected void OnError(string errorMessage)
        {
            OnError(new Exception(errorMessage));
        }

        protected virtual void OnNewClientAccepted(Socket socket, object state)
        {
            var handler = NewClientAccepted;

            if (handler != null)
                handler(this, socket, state);
        }

        protected void OnNewClientAcceptedAsync(Socket socket, object state)
        {
            var handler = NewClientAccepted;

            if (handler != null)
            {
                switch (this.CurrentSocketModel)
                {
                    case SocketMode.Tcp: {
                            handler.BeginInvoke(this, socket, state, null, null);
                        };break;
                    case SocketMode.Udp:
                        {
                            handler.Invoke(this, socket, state);
                        }; break;
                }
                
                
            }
            
        }

        /// <summary>
        /// Occurs when [stopped].
        /// </summary>
        public event EventHandler Stopped;

        protected void OnStopped()
        {
            var handler = Stopped;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
