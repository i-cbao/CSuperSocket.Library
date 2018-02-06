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
    public class UdpSocketClient : UdpSocket
    {
        public UdpSession Session { get; private set; }

        //protected override ILogger Logger = LoggerManager.GetLogger("UdpClient");

        public bool IsConnected { get; private set; }

        public event EventHandler Closed;

        public IPEndPoint RealTarget { get; protected set; }

       // private System.Threading.ManualResetEvent connWait = null;

        private List<UdpSession> proxySessionList = new List<UdpSession>();

        public List<UdpSession> ProxySessionList
        {
            get
            {
                List<UdpSession> proxyList = null;
                lock (proxySessionList)
                {
                    proxyList = proxySessionList.ToList();
                }

                return proxyList;
            }
        }

        private Dictionary<UInt16, ManualResetEvent> connWaitList = new Dictionary<ushort, ManualResetEvent>();

        public IUdpSessionFactory SessionFactory { get; private set; }


        public TimeSpan Timeout { get; set; }

        private long checkTimeInterval = 3000;
        private Timer checkTimer = null;
        private object timerLocker = new object();

        public UdpSocketClient(string targetIP, int targetPort, int localPort)
            : this(targetIP, targetPort, localPort, new DefaultUdpSessionFactory(false))
        {
            
        }

        public UdpSocketClient(string targetIP, int targetPort, int localPort, IUdpSessionFactory sessionFactory)
        {
            Logger = LoggerManager.GetLogger(String.Format("UDPClient_{0}_{1}_{2}", targetIP, targetPort, localPort));
            Target = new DnsEndPoint(targetIP, targetPort);
            IPAddress[] ips  = Dns.GetHostAddresses(targetIP);
            IPAddress ip = ips.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);


            RealTarget = new IPEndPoint(ip, targetPort);
            Port = localPort;
            Session = sessionFactory.CreateSession(this, Target as DnsEndPoint) ;
            IsConnected = false;
            SessionFactory = sessionFactory;
            Timeout = TimeSpan.FromSeconds(15);
            checkTimer = new Timer(new TimerCallback(TimeoutCheck), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        

        public bool ConnectedSync()
        {
            if (IsConnected)
            {
                return IsConnected;
            }
            return ConnectedSync(0);
        }

        protected virtual void TimeoutCheck(object state)
        {
            if (!IsConnected)
            {
                return;
            }

            if ((DateTime.Now - LastReceivedTime).TotalMilliseconds >= Timeout.TotalMilliseconds)
            {
                Logger.Trace("Ping无应答，连接丢失");
                Close();
                return;
            }


            if ((DateTime.Now - LastReceivedTime).TotalMilliseconds >= checkTimeInterval)
            {
                UdpFrame ping = new UdpFrame(0, 0, 0, UdpCommand.Ping, null, 0);
                SendFrame(ping, Target);
            }
            lock (timerLocker)
            {
                if (checkTimer != null)
                {
                    checkTimer.Change(checkTimeInterval, System.Threading.Timeout.Infinite);
                }
            }
        }

        protected override void beginReceive(UdpClient c)
        {

            if (!isReceiving)
                return;

            try
            {
                c.BeginReceive(this.receiveProc, c);
            }
            catch (SocketException e)
            {
                Logger.Error("接收数据发送异常：\r\n{0}", e.ToString());
                Close(false, 0);
            }
            catch (ObjectDisposedException e)
            {
                isReceiving = false;
                Close(false, 0);
            }

        }

      
        protected bool ConnectedSync(UInt16 proxyId)
        {
            try
            {


                if (proxyId == 0)
                {
                    if (Client != null)
                    {
                        Close();
                    }

                    //Client = new System.Net.Sockets.UdpClient(Port);
                    //Client.DontFragment = true;

                    //Client.Client.ReceiveBufferSize = 1024 * 1024 * 2;
                    //Client.Client.SendBufferSize = 1024 * 1024 * 2;
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
                }

                Logger.Trace("发送连接请求：{0}", Target);
                UdpFrame frame = new UdpFrame(0, 0, 0, UdpCommand.Connect, null, proxyId);
                SendFrame(frame, Target);
                ManualResetEvent connWait = new System.Threading.ManualResetEvent(false);
                lock (connWaitList)
                {
                    connWaitList.Add(proxyId, connWait);
                }
                connWait.WaitOne(10000);

                lock (connWaitList)
                {
                    connWaitList.Remove(proxyId);
                }

                if (IsConnected && proxyId == 0)
                {
                    lock (timerLocker)
                    {
                        if (checkTimer == null)
                        {
                            checkTimer = new Timer(new TimerCallback(TimeoutCheck), null, checkTimeInterval, System.Threading.Timeout.Infinite);
                        }
                        else
                        {
                            checkTimer.Change(checkTimeInterval, System.Threading.Timeout.Infinite);
                        }
                    }
                    Logger.Trace("开启Ping");
                }
                else if (!IsConnected && proxyId == 0)
                {
                    if (Client != null)
                    {
                        Client.Close();
                    }
                    StopSend();
                    StopReceive();
                }


                return IsConnected;
            }
            catch (Exception e)
            {
                Logger.Error("UdpSocketClient连接异常：\r\n{0}", e.ToString());
                if (proxyId == 0)
                {
                    if (IsSending)
                    {
                        StopSend();
                    }
                    if (IsReceiving)
                    {
                        StopReceive();
                    }
                }
                
                return false;
            }
        }

        

        public UdpSession CreateProxySession()
        {
            UdpSession session = SessionFactory.CreateSession(this, Target as DnsEndPoint);
            session.ProxyID = GetProxyID();
            lock (proxySessionList)
            {
                proxySessionList.Add(session);
            }

            bool isConnected = ConnectedSync(session.ProxyID);


            if (isConnected)
            {
                return session;
            }
            else
            {
                lock (proxySessionList)
                {
                    proxySessionList.Remove(session);
                }
                return null;
            }

            

        }


        public virtual void Close(UInt16 proxyId)
        {
            Close(true, proxyId);
        }

        public override void Close()
        {
            Close(true, 0);   
        }

        private int mainCloseSignal = 0;
        protected virtual void Close(bool isNotify, UInt16 proxyId)
        {

            if (proxyId == 0)
            {
                if (Interlocked.Exchange(ref mainCloseSignal, 1) == 1)
                {
                    return;
                }
                lock (timerLocker)
                {
                    if (checkTimer != null)
                    {
                        checkTimer.Dispose();
                        checkTimer = null;
                        Logger.Trace("关闭Ping");
                    }
                }

                List<UdpSession> sl = null;
                lock (proxySessionList)
                {
                    sl = proxySessionList.ToList();
                }
                sl.ForEach(s =>
                {
                    Close(isNotify, s.ProxyID);
                });
                if (RealTarget != null)
                {
                    RemoveCachePackage(RealTarget);
                }
            }

            if (isNotify)
            {
                UdpFrame frame = new UdpFrame(0, 0, 0, UdpCommand.Close, null, proxyId);
                try
                {
                    InnerSendFrame(frame, Target);
                }
                catch (SocketException)
                {
                }
            }
            if (Client != null && proxyId == 0)
            {
                Client.Close();
            }
            if (proxyId == 0)
            {
                bool oldConnected = IsConnected;
                IsConnected = false;

                StopReceive();
                StopSend();

                
                if (oldConnected)
                {
                    OnClosed();
                    Session.OnClosed();
                }

                Interlocked.Exchange(ref mainCloseSignal, 0);
            }
            else
            {
                UdpSession proxySession = null;
                lock (proxySessionList)
                {
                    proxySession = proxySessionList.FirstOrDefault(x => x.ProxyID == proxyId);
                    if (proxySession != null)
                    {
                        proxySessionList.Remove(proxySession);
                    }
                }
                if (proxySession != null)
                {
                    proxySession.OnClosed();
                }
            }
           
        }

        public virtual void Send(byte[] data)
        {
            Send(data, 0);
        }

        internal virtual void Send(byte[] data, UInt16 proxyId)
        {
            if (!IsConnected)
            {
                return;
            }

            bool canSend = true;
            if (proxyId != 0)
            {
                lock (proxySessionList)
                {
                    if (proxySessionList.FirstOrDefault(x => x.ProxyID == proxyId) == null )
                    {
                        canSend = false;
                    }
                }
            }
            if (!canSend)
            {
                return;
            }

            Send(data, Target, proxyId, null);
        }

        public override NetStatistic GetStatisticInfo()
        {
            NetStatistic s = base.GetStatisticInfo();

            NetStatisticGroup gs = s.AddGroup("会话信息", false);
            gs.AddColumn("代理ID", "SEQ");
            gs.AddColumn("接收字节数", "BYTE");
            gs.AddColumn("发送字节数", "BYTE");
            gs.AddColumn("错误字节数", "BYTE");
            gs.AddColumn("开始时间","TIME");
            gs.AddColumn("最后接收时间", "TIME");
            gs.AddColumn("最后发送时间", "TIME");

            List<UdpSession> sl = null;
            lock (proxySessionList)
            {
                sl = proxySessionList.ToList();
            }
            foreach (UdpSession session in sl)
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

        public virtual void SendText(string text)
        {
            SendText(text, 0);
        }

        internal virtual void SendText(string text, UInt16 proxyId)
        {
            if (!IsConnected)
            {
                return;
            }

            bool canSend = true;
            if (proxyId != 0)
            {
                lock (proxySessionList)
                {
                    if (proxySessionList.FirstOrDefault(x => x.ProxyID == proxyId) == null)
                    {
                        canSend = false;
                    }
                }
            }
            if (!canSend)
            {
                return;
            }

            SendText(text, Target, proxyId, null);
        }

        public virtual bool SendSync(byte[] data)
        {
            return SendSync(data, 0);
        }

        internal virtual bool SendSync(byte[] data, UInt16 proxyId)
        {
            if (!IsConnected)
            {
                return false;
            }
            bool canSend = true;
            if (proxyId != 0)
            {
                lock (proxySessionList)
                {
                    if (proxySessionList.FirstOrDefault(x => x.ProxyID == proxyId) == null)
                    {
                        canSend = false;
                    }
                }
            }
            if (!canSend)
            {
                return false;
            }
            ManualResetEvent wait = new ManualResetEvent(false);
            bool isSuccess = false;
            Send(data, Target, proxyId, (up, r) =>
            {
                isSuccess = r;
                wait.Set();
            });
            wait.WaitOne(SendTimeout);
            return isSuccess;
        }


        public virtual bool SendTextSync(string text)
        {
            return SendTextSync(text, 0);
        }

        internal virtual bool SendTextSync(string text, UInt16 proxyId)
        {
            if (!IsConnected)
            {
                return false;
            }
            bool canSend = true;
            if (proxyId != 0)
            {
                lock (proxySessionList)
                {
                    if (proxySessionList.FirstOrDefault(x => x.ProxyID == proxyId) == null)
                    {
                        canSend = false;
                    }
                }
            }
            if (!canSend)
            {
                return false;
            }
            ManualResetEvent wait = new ManualResetEvent(false);
            bool isSuccess = false;
            SendText(text, Target, proxyId, (up, r) =>
            {
                isSuccess = r;
                wait.Set();
            });
            wait.WaitOne(SendTimeout);
            return isSuccess;
        }

        protected override void SendData(byte[] data, EndPoint target, UInt16 proxyId)
        {
            try
            {
                base.SendData(data, Target, proxyId);
            }
            catch (SocketException e)
            {
                Close(false,proxyId);
            }
        }


       

       

        protected override UdpFrame OnReceivedFrameData(byte[] data, IPEndPoint ep)
        {
            if (data == null || data.Length < UdpFrame.HeadLength)
                return null;


            UdpFrame frame = new UdpFrame(data);
            if (frame.Command == UdpCommand.ConnectConfirm)
            {
                Logger.Trace("收到连接确认应答帧：{0} {1}", ep, frame.ProxyID);
                if (frame.ProxyID == 0)
                {
                    RemoveCachePackage(ep);
                    //RealTarget = ep;
                    IsConnected = true;
                }
                ManualResetEvent wait = null;
                lock (connWaitList)
                {
                    if (connWaitList.ContainsKey(frame.ProxyID))
                    {
                        wait = connWaitList[frame.ProxyID];
                    }
                }
                if (wait != null)
                {
                    wait.Set();
                }
                
                return null;
            }
            else if (frame.Command == UdpCommand.Close)
            {
                Logger.Trace("服务端关闭会话：{0}", ep);
                Close(false, frame.ProxyID);
            }
            else if (frame.Command == UdpCommand.UnConnected)
            {
                Logger.Trace("服务端返回未连接：{0}", ep);
                Close(false, frame.ProxyID);
            }
            else if (frame.Command == UdpCommand.Ping)
            {
                if (IsConnected)
                {
                    UdpFrame pong = new UdpFrame(0, 0, 0, UdpCommand.Pong, null, frame.ProxyID);
                    SendFrame(pong, ep);
                }
            }

            return frame;
        }
        

        protected override void OnReceiveError(Exception exception)
        {
            Logger.Trace("接收错误,关闭");
            Close(false, 0);
        }

        protected virtual void OnClosed()
        {
            EventHandler h = Closed;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }


        protected override void OnReceivedData(byte[] data, IPEndPoint ep, UInt16 proxyId, bool isText)
        {
            UdpSession session = getSession(proxyId);
            if (session == null)
                return;

            OnReceivedData(new ReceivedDataEventArgs(data, session, isText));
            session.RaiseReceivedData(data, isText);
            lock (session)
            {
                session.ReceivedBytes += (data == null ? 0 : data.Length);
                session.LastReceiveTime = DateTime.Now;
                session.ActiveTime = DateTime.Now;
            }
            session = getSession(0);
            if (session != null)
            {
                lock (session)
                {
                    session.ActiveTime = DateTime.Now;
                }
            }
        }

        protected override void OnSendedPackage(UdpPackage package, EndPoint target, UInt16 proxyId)
        {
            UdpSession session = getSession(proxyId);
            if (session == null)
                return;
            lock (session)
            {
                session.SendedBytes += (package.Data == null ? 0 : package.Data.Length);
                session.ActiveTime = DateTime.Now;
                session.LastSendTime = DateTime.Now;
            }
            session = getSession(0);
            if (session != null)
            {
                lock (session)
                {
                    session.ActiveTime = DateTime.Now;
                }
            }
        }

        private UdpSession getSession(UInt16 proxyId)
        {
            UdpSession session = null;
            if (proxyId == 0)
            {
                session = Session;
            }
            else
            {
                lock (proxySessionList)
                {
                    session = proxySessionList.FirstOrDefault(x => x.ProxyID == proxyId);
                }
            }
            return session;

        }

        protected override void OnSendPackageError(UdpPackage package, EndPoint target, UInt16 proxyId)
        {
            UdpSession session = getSession(proxyId);
            if (session == null)
                return;
            lock (session)
            {
                session.ErrorBytes += (package.Data == null ? 0 : package.Data.Length);
            }
        }
    }
}
