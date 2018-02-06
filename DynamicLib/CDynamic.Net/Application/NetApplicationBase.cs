using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using Dynamic.Core.Cache;
using Dynamic.Core.Log;
using System.Threading;
using Dynamic.Net.Session;

namespace Dynamic.Net.Application
{
    public class NetApplicationBase : INetApplication
    {

        private ILogger logger = null;
        protected ILogger Logger
        {
            get
            {
                return logger;
            }
        }

        private Timer checkTimer = null;
        private object checkLock = new object();

        private TimeSpan checkTimeoutInterval = TimeSpan.FromMinutes(5);
        public TimeSpan CheckTimeoutInterval
        {
            get
            {
                return checkTimeoutInterval;
            }
            set
            {
                if (checkTimeoutInterval != value)
                {
                    checkTimeoutInterval = value;
                    if (checkTimer != null)
                    {
                        checkTimer.Change(value, value);
                    }
                }
            }
        }

        public event EventHandler SessionStarted;
       

        public event EventHandler SessionClosed;

        public Dictionary<String, INetSession> SessionList = new Dictionary<string, INetSession>();

        public object SyncLocker = new object();

        public bool IsStarted { get; set; }

        public virtual bool Setup(string appName)
        {

            this.Name = appName;
            this.appGuid = Guid.NewGuid().ToString("N").ToLower();

            this.serverList = new List<INetServer>();

            logger.Info("Application Setup：{0} {1}", appName, appGuid);


            return true;
        }

        private DateTime lastCheckedTime = DateTime.Now;
        protected virtual void CheckTimeout(object ctx)
        {
            logger.Trace("开始检查超时会话");
            lock (SyncLocker)
            {
                List<INetSession> timeoutSession = new List<INetSession>();
                if (SessionList != null && SessionList.Any())
                {
                    lock (SyncLocker)
                    {
                        foreach (KeyValuePair<string, INetSession> kv in SessionList)
                        {
                            if (kv.Value.IsTimeout())
                            {
                                timeoutSession.Add(kv.Value);
                            }
                        }
                    }
                }
                if (timeoutSession.Any())
                {
                    timeoutSession.All(x =>
                    {
                        try
                        {
                            if (x.Close())
                            {
                                RemoveSession(x);
                                logger.Debug("关闭超时会话：{0}", x.SessionID);
                            }
                            else
                            {
                                logger.Error("未能成功关闭超时会话：{0}", x.SessionID);
                            }
                        }
                        catch(Exception e)
                        {
                            logger.Error("关闭超时会话时发生异常：" + e.ToString());
                        }
                        return true;
                    });
                }
            }
        }

        public bool Start()
        {
            bool isAllSuccess = true;
            foreach (INetServer server in serverList)
            {
                try
                {
                    if (!server.Start())
                    {
                        logger.Error("启动服务失败：{0}", server.GetType().FullName);
                        isAllSuccess = false;
                    }
                    else
                    {

                    }
                }
                catch (Exception e)
                {
                    logger.Error("启动服务失败：{0}", e.ToString());
                }
            }

            IsStarted = true;
            checkTimer = new Timer(CheckTimeout, null, CheckTimeoutInterval, CheckTimeoutInterval);
            return isAllSuccess;
        }

        public bool Stop()
        {
            if (SessionList.Any())
            {
                List<INetSession> sl = new List<INetSession>();
                foreach (KeyValuePair<string, INetSession> kv in SessionList)
                {
                    if (kv.Value != null)
                    {
                        sl.Add(kv.Value);
                    }
                }
                sl.All(x =>
                {
                    try
                    {
                        x.Close();
                    }
                    catch { }
                    return true;
                });
            }
            

            foreach (INetServer server in serverList)
            {
                try
                {
                    if (!server.Stop())
                    {
                        logger.Error("启动服务失败：{0}", server.GetType().FullName);
                    }
                    else
                    {

                    }
                }
                catch (Exception e)
                {
                    logger.Error("启动服务失败：{0}", e.ToString());
                }
            }
            checkTimer.Dispose();
            checkTimer = null;
            IsStarted = false;
            return true;
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;
                    if (!String.IsNullOrEmpty(value))
                    {
                        logger = LoggerManager.GetLogger(String.Format("App.{0}", value));
                    }
                }
            }
        }

        private string appGuid;
        public string AppGuid
        {
            get { return appGuid; }
        }

        protected List<INetServer> serverList = null;
        public IEnumerable<INetServer> ServerList
        {
            get { return serverList; }
        }

        public void AddServer(INetServer server)
        {
            if (serverList == null)
            {
                serverList = new List<INetServer>();
            }

            serverList.Add(server);
        }

        public void RemoveServer(INetServer server)
        {
            if (serverList != null)
            {
                serverList.Remove(server);
            }
        }

        public void ClearServer()
        {
            if (serverList != null)
            {
                serverList.Clear();
            }
        }


        public INetSession GetSession(string sessionID)
        {
            if (SessionList.ContainsKey(sessionID))
            {
                return SessionList[sessionID];
            }

            return null;
        }

        public virtual NetStatistic GetStatisticInfo()
        {
            NetStatistic s = new NetStatistic();

            NetStatisticGroup gs = s.AddGroup("会话信息", false);
            gs.AddColumn("接收字节数", "BYTE");
            gs.AddColumn("发送字节数", "BYTE");
            gs.AddColumn("错误字节数", "BYTE");
            gs.AddColumn("活动时间", "TIME");
            gs.AddColumn("开始时间", "TIME");
         
            List<INetSession> sl = null;
            lock (SyncLocker)
            {
                sl = SessionList.Values.ToList();
            }
            foreach (INetSession session in sl)
            {
                SocketSession ss = session as SocketSession;
                if (ss != null)
                {
                    string sn = string.Format("{0},{1}", session.SessionID, ss.EndPoint.ToString());
                    NetStatisticItem si = gs.AddItem(sn, ss.ReceivedBytes);
                    si.Value2 = ss.SendedBytes;
                    si.Value3 = ss.ErrorBytes;
                    si.Value4 = ss.ActiveTime.Ticks;
                    si.Value5 = ss.StartTime.Ticks;

                }
            }

            return s;
        }

        public bool SetSession(INetSession session)
        {
            lock (SyncLocker)
            {
                if (SessionList.ContainsKey(session.SessionID))
                {
                    SessionList[session.SessionID] = session;
                }
                else
                {
                    SessionList.Add(session.SessionID, session);
                }
            }
            return true;
        }

        public void RemoveSession(INetSession session)
        {
            lock (SyncLocker)
            {
                if (session != null && SessionList.ContainsKey(session.SessionID))
                {
                    SessionList.Remove(session.SessionID);
                    session.SessionStarted -= session_SessionStarted;
                    session.SessionClosed -= session_SessionClosed;
                }
            }
        }

        public int SessionCount
        {
            get
            {
                int count = 0;
                lock (SyncLocker)
                {
                    count = SessionList.Count();
                }
                return count;
            }
        }

        public object GetCache(string cacheKey)
        {
            return CacheManagerUnity.Get(Name, cacheKey);
        }

        public bool SetCache(string cacheKey, object value)
        {
            CacheManagerUnity.Add(Name, cacheKey, value);
            return true;
        }

        public virtual void SessionCreated(INetSession session)
        {
            session.SessionStarted -= session_SessionStarted;
            session.SessionStarted += new EventHandler(session_SessionStarted);

            session.SessionClosed -= session_SessionClosed;
            session.SessionClosed += new EventHandler(session_SessionClosed);
        }

        public virtual void Broadcast(INetCommand command)
        {
            lock (SyncLocker)
            {
                Broadcast(command, (x) => { return true; });
            }
        }

        public void Broadcast(INetCommand command, Func<INetSession, bool> check)
        {
            List<INetSession> bcList = new List<INetSession>();
            lock (SyncLocker)
            {
                foreach (KeyValuePair<string, INetSession> kv in SessionList)
                {
                    if (check(kv.Value))
                    {
                        bcList.Add(kv.Value);
                    }
                }
            }
            bcList.All(x =>
            {
                if (x != null)
                {
                    try
                    {

                        x.Protocol.WriteCommand(command, x);
                    }

                    catch (Exception e)
                    {
                        logger.Warn("广播消息失败：{0}", x.SessionID);
                    }
                }

                return true;
            });
        }

        void session_SessionClosed(object sender, EventArgs e)
        {
            RemoveSession(sender as INetSession);

            if (SessionClosed != null)
            {
                SessionClosed(sender, e);
            }
        }

        void session_SessionStarted(object sender, EventArgs e)
        {
            if (sender is INetSession)
            {
                SetSession(sender as INetSession);

                if (SessionStarted != null)
                {
                    SessionStarted(sender, e);
                }
            }
        }


    }
}
