using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Dynamic.Core.Log;

namespace Dynamic.Net
{
    public delegate void UdpClientConnectedHandler(UdpSocketServer server, UdpSession session);

    public class UdpSocketServer : UdpSocket
    {
        //protected ILogger Logger = LoggerManager.GetLogger("UdpServer");
        public bool IsListening { get; private set; }

        private List<UdpServerSession> clientList = new List<UdpServerSession>();

        public List<UdpSession> ClientList
        {
            get
            {
                List<UdpSession> list = null;
                lock (clientList)
                {
                    list = clientList.ToList().OfType<UdpSession>().ToList();
                }

                return list;
            }
        }

        public event UdpClientConnectedHandler ClientConnected;
        public event UdpClientConnectedHandler ClientClosed;

        private System.Threading.ManualResetEvent pingWait = null;

        //public TimeSpan PingInterval { get; set; }

        public TimeSpan Timeout { get; set; }

        public IUdpSessionFactory SessionFactory { get; protected set; }

        public UdpSocketServer(int port)
            : this(port, new DefaultUdpSessionFactory(true))
        {

        }

        public UdpSocketServer(int port, IUdpSessionFactory sf)
        {
            Logger = LoggerManager.GetLogger("UdpServer");
            Target = new IPEndPoint(IPAddress.Any, port);
            Port = port;
            Timeout = TimeSpan.FromSeconds(15);
            //PingInterval = TimeSpan.FromSeconds(5);
            SessionFactory = sf;
        }

        public virtual void Listening()
        {
            if (IsListening)
            {
                return;
            }
            //Client = new System.Net.Sockets.UdpClient(Port);
            //Client.DontFragment = true;
            Client = new UdpClient();
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            Client.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false)      
}, null);   

            Client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
            Client.Client.DontFragment = true;
            Client.Client.ReceiveBufferSize = 1024 * 1024 * 2;
            Client.Client.SendBufferSize = 1024 * 1024 * 2;

            StartReceive();
            StartSend();
            IsListening = true;


            ThreadPool.QueueUserWorkItem(new WaitCallback((c) =>
            {
                pingProc();
            }));
        }

        public virtual void Stop()
        {
            StopReceive();
            StopSend();

            List<UdpServerSession> sl = null;
            lock (clientList)
            {
                sl = clientList.Where(x => x.ProxyID == 0).ToList();
            }
            sl.ForEach(s =>
            {
                CloseClient(s.Target, s.ProxyID);
            });
            Client.Close();
            IsListening = false;
            pingWait = new System.Threading.ManualResetEvent(false);
            pingWait.WaitOne(3000);
        }

        public override void Close()
        {
            Stop();
        }

        public virtual void Send(byte[] data, EndPoint target)
        {
            Send(data, target, 0, null);
        }

        internal virtual void Send(byte[] data, EndPoint target, UInt16 proxyId)
        {
            Send(data, target, proxyId, null);
        }

        public virtual void SendText(string text, EndPoint target)
        {
            SendText(text, target, 0);
        }

        internal virtual void SendText(string text, EndPoint target, UInt16 proxyId)
        {
            SendText(text, target, proxyId, null);
        }

        public virtual bool SendSync(byte[] data, EndPoint target)
        {
            return SendSync(data, target, 0);
        }

        internal virtual bool SendSync(byte[] data, EndPoint target, UInt16 proxyId)
        {


            ManualResetEvent wait = new ManualResetEvent(false);
            bool isSuccess = false;
            Send(data, target, proxyId, (up, r) =>
            {
                isSuccess = r;
                wait.Set();
            });
            wait.WaitOne(SendTimeout);
            return isSuccess;
        }


        public virtual bool SendTextSync(string text, EndPoint target)
        {
            return SendTextSync(text, target, 0);
        }

        internal virtual bool SendTextSync(string text, EndPoint target, UInt16 proxyId)
        {


            ManualResetEvent wait = new ManualResetEvent(false);
            bool isSuccess = false;
            SendText(text, target, proxyId, (up, r) =>
            {
                isSuccess = r;
                wait.Set();
            });
            wait.WaitOne(SendTimeout);
            return isSuccess;
        }

        public void CloseClient(EndPoint ep, UInt16 proxyId)
        {
            if (proxyId == 0)
            {
                List<UdpServerSession> sl = getSession(ep);
                UdpServerSession mainSession = null;
                sl.ForEach(s =>
                {
                    if (s.ProxyID != 0)
                    {
                        CloseClient(s.Target, s.ProxyID);
                    }
                    else
                    {
                        mainSession = s;
                    }
                });

                if (mainSession != null)
                {
                    RemoveSession(mainSession);
                }
            }
            else
            {
                UdpServerSession session = getSession(ep, proxyId);
                if (session != null)
                {
                    RemoveSession(session);

                }
            }
        }

        protected virtual void RemoveSession(UdpServerSession session)
        {
            UdpServerSession old = getSession(session.Target.Host, session.Target.Port, session.ProxyID);
            if (old != null)
            {
                Logger.Trace("移除会话：{0} : {1} {2}", session.Target.Host, session.Target.Port, session.ProxyID);

                if (old.ProxyID == 0)
                {
                    RemoveCachePackage(new IPEndPoint(IPAddress.Parse(old.Target.Host), old.Target.Port));
                }

                old.Client.Close();
                lock (clientList)
                {
                    clientList.Remove(old);
                }
                RemoveSendQueue(session.Target, session.ProxyID);
                OnClientClosed(session);
                session.OnClosed();
            }
        }

        public override NetStatistic GetStatisticInfo()
        {
            NetStatistic s = base.GetStatisticInfo();

            NetStatisticGroup gs = s.AddGroup("会话信息", false);
            gs.AddColumn("代理ID", "SEQ");
            gs.AddColumn("接收字节数", "BYTE");
            gs.AddColumn("发送字节数", "BYTE");
            gs.AddColumn("错误字节数", "BYTE");
            gs.AddColumn("开始时间", "TIME");
            gs.AddColumn("最后接收时间", "TIME");
            gs.AddColumn("最后发送时间", "TIME");

            List<UdpServerSession> sl = null;
            lock (clientList)
            {
                sl = clientList.ToList();
            }
            foreach (UdpServerSession session in sl)
            {
                string sn = string.Format("{0},{1},{2}", session.SessionID, session.Target.ToString(), session.Url ?? "");
                NetStatisticItem si = gs.AddItem(sn, session.ProxyID);
                si.Value2 = session.ReceivedBytes;
                si.Value3 = session.SendedBytes;
                si.Value4 = session.ErrorBytes;
                si.Value5 = session.StartTime.Ticks;
                si.Value6 = session.LastReceiveTime.Ticks;
                si.Value7 = session.LastSendTime.Ticks;
            }

            return s;
        }


        public int ConnectedCount
        {
            get { return clientList.Count; }
        }

        protected override UdpFrame OnReceivedFrameData(byte[] data, System.Net.IPEndPoint ep)
        {
            if (data == null || data.Length < UdpFrame.HeadLength)
                return null;

            UdpFrame frame = new UdpFrame(data);
            if (frame.Command == UdpCommand.Connect)
            {
                RemoveCachePackage(ep);
                ILogger l = LoggerManager.GetLogger(ep.Address.ToString() + "_" + ep.Port);
                l.Trace("收到连接命令：{0}", ep.ToString());
                //Logger.Trace("收到连接命令：{0}", ep.ToString());

                //连接成功
                UdpServerSession session = null;
                bool isNew = false;
                lock (clientList)
                {
                    session = clientList.FirstOrDefault(x => x.Target.Host == ep.Address.ToString() && ep.Port == x.Target.Port && x.ProxyID == frame.ProxyID);
                    if (session == null)
                    {
                        session = SessionFactory.CreateSession(null, new DnsEndPoint(ep.Address.ToString(), ep.Port)) as UdpServerSession;
                        // session = new UdpServerSession();
                        session.StartTime = DateTime.Now;
                        session.ProxyID = frame.ProxyID;
                        //session.Target = new DnsEndPoint(ep.Address.ToString(), ep.Port);
                        session.Client = new UdpServerSocketProxy(this, Client, session);
                        session.StartTime = DateTime.Now;
                        session.ActiveTime = DateTime.Now;

                        clientList.Add(session);
                        isNew = true;
                        l.Trace("添加会话");
                    }
                    else
                    {
                        session.Client = null;
                        session.ProxyID = frame.ProxyID;
                        session.Target = new DnsEndPoint(ep.Address.ToString(), ep.Port);
                        session.Client = new UdpServerSocketProxy(this, Client, session);
                        session.Client.IsConnected = true;
                        l.Trace("更新会话");
                    }
                    if (isNew)
                    {
                        OnClientConnected(session);
                    }
                }
                UdpFrame ccFrame = new UdpFrame(0, 0, 0, UdpCommand.ConnectConfirm, null, frame.ProxyID);
                SendFrame(ccFrame, session.Target);
                return null;
            }
            else if (frame.Command == UdpCommand.Close)
            {
                ILogger l = LoggerManager.GetLogger(ep.Address.ToString() + "_" + ep.Port);
                UdpServerSession session = getSession(ep.Address.ToString(), ep.Port, frame.ProxyID);
                if (session != null)
                {
                    session.Client.IsConnected = false;
                    l.Trace("收到回话关闭命令：{0}", session.Target);
                    RemoveSession(session);
                }
            }
            else if (frame.Command == UdpCommand.Pong)
            {

                List<UdpServerSession> sl = null;
                lock (clientList)
                {
                    sl = clientList.Where(x => x.Target.Host == ep.Address.ToString() && x.Target.Port == ep.Port).ToList();
                }
                sl.ForEach(s =>
                {
                    s.ActiveTime = DateTime.Now;
                    s.LastReceiveTime = DateTime.Now;
                });
                //UdpServerSession session = getSession(ep.Address.ToString(), ep.Port, frame.ProxyID);
                //if (session != null)
                //{
                //    session.ActiveTime = DateTime.Now;
                //    session.LastReceiveTime = DateTime.Now;
                //}
            }
            else if (frame.Command == UdpCommand.Ping)
            {
                //UdpServerSession session = getSession(ep.Address.ToString(), ep.Port, frame.ProxyID);
                //if (session != null)
                //{
                //    Logger.Trace("收到Ping命令：{0}", session.Target);
                //    UdpFrame pong = new UdpFrame(0, 0, 0, UdpCommand.Pong, null, frame.ProxyID);
                //    SendFrame(pong, ep);

                //}

                UdpFrame pong = new UdpFrame(0, 0, 0, UdpCommand.Pong, null, frame.ProxyID);
                SendFrame(pong, ep);

                List<UdpServerSession> sl = null;
                lock (clientList)
                {
                    sl = clientList.Where(x => x.Target.Host == ep.Address.ToString() && x.Target.Port == ep.Port).ToList();
                }
                sl.ForEach(s =>
                {
                    s.ActiveTime = DateTime.Now;
                    s.LastReceiveTime = DateTime.Now;
                });




            }


            return frame;
        }

        protected override void OnReceivedData(byte[] data, System.Net.IPEndPoint ep, UInt16 proxyId, bool isText)
        {
            UdpServerSession session = getSession(ep.Address.ToString(), ep.Port, proxyId);
            if (session != null)
            {
                lock (session)
                {
                    session.ReceivedBytes += (data == null ? 0 : data.Length);
                    session.ActiveTime = DateTime.Now;
                    session.LastReceiveTime = DateTime.Now;
                }
                OnReceivedData(new ReceivedDataEventArgs(data, session, isText));
                session.RaiseReceivedData(data, isText);
            }
            else
            {
                UdpFrame frame = new UdpFrame(0, 0, 0, UdpCommand.UnConnected, null, proxyId);
                SendFrame(frame, ep);
            }
            //修改主连接会话最后接收时间
            session = getSession(ep.Address.ToString(), ep.Port, 0);
            if (session != null)
            {
                lock (session)
                {
                    session.ActiveTime = DateTime.Now;
                }
            }
        }

        protected override void SendData(byte[] data, EndPoint target, UInt16 proxyId)
        {
            UdpServerSession session = getSession(target, proxyId);
            if (session != null)
            {
                try
                {
                    //使用客户端连接自己的UdpClient发送数据
                    session.Client.SendData(data);
                }
                catch (SocketException e)
                {
                    session.Client.IsConnected = false;
                    session.Close();
                    OnClientClosed(session);
                }
            }
        }

        private UdpServerSession getSession(string host, int port, UInt16 proxyId)
        {
            UdpServerSession session = null;
            lock (clientList)
            {
                session = clientList.FirstOrDefault(x => x.Target.Host == host && x.Target.Port == port && x.ProxyID == proxyId);
            }

            return session;

        }

        private UdpServerSession getSession(EndPoint target, UInt16 proxyId)
        {
            string host = "";
            int port = 0;
            if (target is IPEndPoint)
            {
                IPEndPoint ip = target as IPEndPoint;
                host = ip.Address.ToString();
                port = ip.Port;
            }
            else if (target is DnsEndPoint)
            {
                DnsEndPoint dns = target as DnsEndPoint;
                host = dns.Host;
                port = dns.Port;
            }
            UdpServerSession session = getSession(host, port, proxyId);

            return session;
        }

        private List<UdpServerSession> getSession(EndPoint target)
        {
            string host = "";
            int port = 0;
            if (target is IPEndPoint)
            {
                IPEndPoint ip = target as IPEndPoint;
                host = ip.Address.ToString();
                port = ip.Port;
            }
            else if (target is DnsEndPoint)
            {
                DnsEndPoint dns = target as DnsEndPoint;
                host = dns.Host;
                port = dns.Port;
            }

            List<UdpServerSession> sl = null;
            lock (clientList)
            {
                sl = clientList.Where(x => x.Target.Host == host && x.Target.Port == port).ToList();
            }

            return sl;
        }


        protected virtual void OnClientConnected(UdpServerSession session)
        {
            UdpClientConnectedHandler h = ClientConnected;
            if (h != null)
            {
                h(this, session);
            }
        }

        protected virtual void OnClientClosed(UdpServerSession session)
        {
            UdpClientConnectedHandler h = ClientClosed;
            if (h != null)
            {
                h(this, session);
            }
        }




        protected override void OnSendedPackage(UdpPackage package, EndPoint target, UInt16 proxyId)
        {
            UdpServerSession session = getSession(target, proxyId);
            if (session != null)
            {
                lock (session)
                {
                    session.SendedBytes += (package.Data == null ? 0 : package.Data.Length);
                    session.LastSendTime = DateTime.Now;
                    session.ActiveTime = DateTime.Now;
                }
            }
            base.OnSendedPackage(package, target, proxyId);
        }

        protected override void OnSendPackageError(UdpPackage package, EndPoint target, UInt16 proxyId)
        {
            UdpServerSession session = getSession(target, proxyId);
            if (session != null)
            {
                lock (session)
                {
                    session.ErrorBytes += (package.Data == null ? 0 : package.Data.Length);
                }
            }
            base.OnSendPackageError(package, target, proxyId);
        }

        private void pingProc()
        {
            while (IsListening)
            {


                //DateTime now = DateTime.Now;
                //List<UdpServerSession> sl = null;
                //lock (clientList)
                //{
                //    sl = clientList.Where(x => x.Client.IsConnected &&  (now - x.Client.LastReceivedTime).TotalMilliseconds > Timeout.TotalMilliseconds).ToList();
                //}
                //foreach (UdpServerSession s in sl)
                //{
                //    Logger.Trace("回话超时：{0}", s.Target);
                //    s.Close();
                //}



                Thread.Sleep(3000);

                //var sg =  sl.GroupBy(x => x.Target);
                //foreach (var g in sg)
                //{
                //    UdpFrame ping = new UdpFrame(0, 0, 0, UdpCommand.Ping, null, 0);
                //    if (IsRunning)
                //    {
                //        SendFrame(ping, g.Key);
                //    }

                //    Thread.Sleep(PingInterval);
                //    sl = sl.Where(x => (now - x.ActiveTime) >= PingInterval).ToList();
                //    sl.ForEach(s =>
                //    {
                //        s.Close();
                //    });
                //}


            }

            System.Threading.ManualResetEvent pw = pingWait;
            if (pw != null)
            {
                pw.Set();
            }
        }


    }
}
