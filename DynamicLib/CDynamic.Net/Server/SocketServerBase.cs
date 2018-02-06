using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Dynamic.Core.Log;
using Dynamic.Net.Session;

namespace Dynamic.Net.Server
{
    /// <summary>
    /// 基于Socket的网络服务
    /// </summary>
    public abstract class SocketServerBase : INetServer, IDisposable
    {
        #region INetServer 成员
        public event EventHandler Started;

        private INetProtocol protocol;
        public INetProtocol Protocol
        {
            get
            {
                return protocol;
            }
        }

        private EndPoint endPoint = null;
        public EndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
        }

        public SocketServerConfig Config
        {
            get
            {
                return config as SocketServerConfig;
            }
        }

        public bool IsRunning { get; set; }

        protected INetServerConfig config;

        protected Socket serverSocket = null;

        protected ILogger Logger = null;

        private ManualResetEvent startupEvent = new ManualResetEvent(false);

        private NetServerStatus status = NetServerStatus.Uninit;
        public NetServerStatus Status
        {
            get
            {
                return status;
            }
            protected set
            {
                status = value;
            }
        }

        public DateTime StartTime { get; set; }

        private INetApplication application = null;
        public INetApplication Application
        {
            get { return application; }
        }

        protected ISocketSessionFactory sessionFactory = null;

        public virtual bool Setup(INetServerConfig config, INetApplication application, INetProtocol protocol)
        {
            return false;
        }

        public virtual bool Setup(INetServerConfig config, INetApplication application, INetProtocol protocol, ISocketSessionFactory sessionFactory)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            this.protocol = protocol;
            this.application = application;
            this.config = config;
            this.endPoint = new IPEndPoint(config.Address, config.Port);
            this.sessionFactory = sessionFactory;

            this.status = NetServerStatus.Inited;
            this.IsRunning = false;

            return true;
        }

        protected abstract bool InnerStart();

        public virtual bool Start()
        {
            if (status == NetServerStatus.Started )
            {
                return false;
            }

            if (status == NetServerStatus.Uninit)
            {
                throw new InvalidOperationException("服务尚未初始化，无法启动");
            }

            if (status == NetServerStatus.Starting)
            {
                return false;
            }

            status = NetServerStatus.Starting;
            startupEvent.Reset();

            Thread thread = new Thread(new ThreadStart(() =>
            {
                bool isSuccess = InnerStart();
                if (isSuccess)
                {
                    status = NetServerStatus.Started;
                }
                else
                {
                    status = NetServerStatus.Error;
                }
                startupEvent.Set();
            }));

            thread.IsBackground = true;
            thread.Start();


            startupEvent.WaitOne();

            StartTime = DateTime.Now;

            if (Started != null)
            {
                Started(this, EventArgs.Empty);
            }

            return status == NetServerStatus.Started;
        }

        protected virtual void StartupCompleted()
        {
            startupEvent.Set();
        }

        public virtual bool Stop()
        {
            if (status == NetServerStatus.Started && serverSocket != null )
            {
                status = NetServerStatus.Stopping;
                serverSocket.Close();

                status = NetServerStatus.Stopped;
            }

            return true;
        }

        #endregion



        #region IDisposable 成员

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }

        #endregion
    }
}
