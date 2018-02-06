using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Dynamic.Net
{
    public class UdpServerSocketProxy : UdpSocket
    {
        public UdpSocketServer Server { get; private set; }

        public UdpSession Session { get; private set; }

        public event EventHandler Closed;

        private bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                IsSending = value;
                IsReceiving = value;
            }
        }

        public UdpServerSocketProxy(UdpSocketServer server,UdpClient  client, UdpSession session)
        {
            Server = server;
            Session = session;
            IsConnected = true;
            Client = client;
            //Client = new UdpClient(sourcePort);
            //Client.DontFragment = true;
          //  Client = new UdpClient();
            FrameWrapper.AddRange(server.FrameWrapper);
            Target = session.Target;
        }

        public void Send(byte[] data)
        {
            Send(data, 0);
        }

        internal void Send(byte[] data, UInt16 proxyId)
        {
            if (!IsConnected)
            {
                return;
            }
            

            Server.Send(data, Session.Target, proxyId);
        }

        internal void SendText(string text, UInt16 proxyId)
        {
            if (!IsConnected)
            {
                return;
            }
            Server.SendText(text, Session.Target, proxyId);
        }

        internal bool SendSync(byte[] data, UInt16 proxyId)
        {
            if (!isConnected)
            {
                return false;
            }
            return Server.SendSync(data, Target, proxyId);
        }

        internal bool SendTextSync(string text, UInt16 proxyId)
        {
            if (!isConnected)
            {
                return false;
            }
            return Server.SendTextSync(text, Target, proxyId);
        }

        public override DateTime LastReceivedTime
        {
            get
            {
                if (Server != null)
                {
                    return Server.LastReceivedTime;
                }
                return base.LastReceivedTime;
            }
            protected set
            {
                //base.LastReceivedTime = value;
            }
        }

        public override void Close()
        {
            if (IsConnected)
            {
                Logger.Trace("关闭会话：{0}", Target);
                Logger.Trace(Environment.StackTrace);
                UdpFrame frame = new UdpFrame(0, 0, 0, UdpCommand.Close, null, Session.ProxyID);
                try
                {
                    InnerSendFrame(frame, Target);
                }
                catch (Exception e)
                {
                }
            }
            //Client.Close();
            bool oldConnected = IsConnected;
            IsConnected = false;
            if (oldConnected)
            {
                OnClosed();
            }
            IsConnected = false;
        }

        protected virtual void OnClosed()
        {
            EventHandler h = Closed;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        internal void SendData(byte[] data)
        {

            if (!IsConnected)
            {
                return;
            }
            SendData(data, Session.Target, Session.ProxyID);

        }

        protected override void OnReceivedData(byte[] data, System.Net.IPEndPoint ep, UInt16 proxyId,bool isText)
        {
            
        }
    }
}
