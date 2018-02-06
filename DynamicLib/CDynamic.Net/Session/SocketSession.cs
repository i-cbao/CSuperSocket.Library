using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace Dynamic.Net.Session
{
    public class SocketSession : INetSession
    {
        private INetApplication application = null;
        private INetProtocol protocol = null;
        private INetServer server = null;
        protected Socket clientSocket = null;
        private string sessionID = "";
        private DateTime startTime;
        protected bool isClosed = false;
        private Encoding encoding = Encoding.UTF8;
        protected Stream InnerStream = null;

        private Dictionary<string, object> sessionData = new Dictionary<string, object>();

        public event EventHandler SessionClosed;

        public event EventHandler SessionStarted;

        public SessionTimeoutType TimeoutType
        {
            get;
            set;
        }

        public double ReceivedBytes { get; set; }

        public double SendedBytes { get; set; }

        public double ErrorBytes { get; set; }


        public SessionCloseReason CloseReason { get; protected set; }

        public SocketSession(INetApplication application, INetProtocol protocol, INetServer server, Socket clientSocket)
        {
            Init(application, protocol, server, clientSocket);
        }

        protected virtual void Init(INetApplication application, INetProtocol protocol, INetServer server, Socket clientSocket)
        {
            this.application = application;
            this.protocol = protocol;
            this.server = server;
            this.clientSocket = clientSocket;
            this.sessionID = Guid.NewGuid().ToString("N").ToLower();
            this.startTime = DateTime.Now;
            this.ActiveTime = DateTime.Now;
            this.Timeout = TimeSpan.FromMinutes(10);
            this.LastRequestTime = DateTime.Now;
            this.LastResponseTime = DateTime.Now;
            this.TimeoutType = SessionTimeoutType.Active;

            if (clientSocket != null)
            {
                InnerStream = new NetworkStream(this.clientSocket);
            }
        }

        public object this[string key]
        {
            get
            {
                if (sessionData.ContainsKey(key))
                {
                    return sessionData[key];
                }
                return null;
            }
            set
            {
                if (sessionData.ContainsKey(key))
                {
                    sessionData[key] = value;
                }
                else
                {
                    sessionData.Add(key, value);
                }
            }
        }

        public void Remove(string key)
        {
            if (sessionData.ContainsKey(key))
            {
                sessionData.Remove(key);
            }
        }

        public EndPoint EndPoint
        {
            get
            {
                var c = clientSocket;
                if (c != null)
                {
                    try
                    {
                        return c.RemoteEndPoint;
                    }
                    catch { }

                    try
                    {
                        return c.LocalEndPoint;
                    }
                    catch { }
                }
                return null;
            }
        }

        public virtual bool IsTimeout()
        {
            if (clientSocket != null && !IsConnected()  )
            {
                return true;
            }

            if (TimeoutType == SessionTimeoutType.Request)
            {
                return (DateTime.Now - LastRequestTime) > Timeout;
            }
            else if (TimeoutType == SessionTimeoutType.Response)
            {
                return (DateTime.Now - LastResponseTime) > Timeout;
            }
            else if (TimeoutType == SessionTimeoutType.Active)
            {
                return (DateTime.Now - (LastResponseTime > LastRequestTime ? LastResponseTime : LastRequestTime)) > Timeout;
            }
            return false;

        }

        #region INetSession 成员

        public Encoding Encoding
        {
            get
            {
                return encoding ?? Encoding.UTF8;
            }
            set
            {
                encoding = value;
            }
        }

        public INetApplication Application
        {
            get { return application; }
        }

        public INetProtocol Protocol
        {
            get { return protocol; }
        }

        public string SessionID
        {
            get { return sessionID; }
            protected set
            {
                sessionID = value;
            }
        }

        public DateTime StartTime
        {
            get { return startTime; }
        }

        public INetServer Server
        {
            get { return server; }
        }

        public Socket ClientSocket
        {
            get { return clientSocket; }
        }

        protected virtual void OnSessionStarted()
        {
            if (SessionStarted != null)
            {
                SessionStarted(this, EventArgs.Empty);
            }

        }

        protected virtual void OnSessionClosed()
        {
            if (SessionClosed != null)
            {
                SessionClosed(this, EventArgs.Empty);
            }
        }

        public virtual void Start()
        {

            OnSessionStarted();

            while (true)
            {
                if (isClosed)
                    break;
                INetCommand command = protocol.GetCommand(this);
                if (isClosed)
                    break;
                if (command != null)
                {
                    command = command.Execute(this);
                }
                protocol.WriteCommand(command, this);
            }
        }

        public virtual bool Close()
        {
            if (clientSocket != null && !isClosed )
            {
                this.isClosed = true;
                clientSocket.Close();
                clientSocket = null;
                OnSessionClosed();
            }

            return true;
        }

        protected virtual void Offline()
        {
            Debug.WriteLine("Socket Session Offline 脱机");
            if (!isClosed)
            {
                CloseReason = SessionCloseReason.Offline;
            }
            Close();
        }

        public virtual bool ReadBytes(byte[] data, int start, int count)
        {
            Byte[] tmpData = new Byte[count];
            try
            {
                ActiveTime = DateTime.Now;
                LastRequestTime = DateTime.Now;
                int rc = InnerStream.Read(data, start, count);

                if (rc == -1)
                {
                    return false;
                }

                ReceivedBytes += rc;
            }
            catch (IOException e)
            {
                if (e.InnerException is SocketException)
                {
                    var se = e.InnerException as SocketException;
                    if (se.ErrorCode == 10004 || se.ErrorCode == 10053 || se.ErrorCode == 10054 || se.ErrorCode == 10058)
                    {
                        this.Offline();
                        return false;
                    }
                }
                else if (e.InnerException is ObjectDisposedException)
                {
                    this.Close();
                    return false;
                }
                else
                {
                    this.Offline();
                    return false;
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10004 || e.ErrorCode == 10053 || e.ErrorCode == 10054 || e.ErrorCode == 10058)
                {
                    Offline();
                }
                else
                {
                    Offline();
                }
                return false;
            }
            catch (ObjectDisposedException e)
            {
                return false;
            }
           
            return true;
        }

        public virtual bool WriteBytes(byte[] data, int start, int count)
        {
            try
            {
                ActiveTime = DateTime.Now;
                LastResponseTime = DateTime.Now;
                InnerStream.Write(data, start, count);
                SendedBytes += count;
            }
            catch (IOException e)
            {
                if (e.InnerException is SocketException)
                {
                    var se = e.InnerException as SocketException;
                    if (se.ErrorCode == 10004 || se.ErrorCode == 10053 || se.ErrorCode == 10054 || se.ErrorCode == 10058)
                    {
                        this.Offline();
                        return false;
                    }
                }
                else if (e.InnerException is ObjectDisposedException)
                {
                    this.Close();
                    return false;
                }
                else
                {
                    this.Offline();
                    return false;
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10004 || e.ErrorCode == 10053 || e.ErrorCode == 10054 || e.ErrorCode == 10058)
                {
                    Offline();
                }
                else
                {
                    Offline();
                }
                return false;
            }
            catch (ObjectDisposedException e)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 检查连接的即时状态
        /// </summary>
        /// <remarks>不能直接通过Socket.Connected属性来检查是否处于连接状态</remarks>
        /// <returns></returns>
        public virtual bool IsConnected()
        {
            if (clientSocket == null)
                return false;

            bool isConnected = false;

            // 通过非阻塞调用来确定连接状态
            bool blockingState = clientSocket.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                clientSocket.Blocking = false;
                clientSocket.Send(tmp, 0, 0);
                Debug.WriteLine("处于连接状态");
                isConnected = true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    Debug.WriteLine("处于连接状态，但是发送数据可能被阻塞");
                    isConnected = true;
                }
                else
                {

                    isConnected = false;
                }
            }
            catch (ObjectDisposedException e)
            {
                isConnected = false;
            }
            finally
            {
                Socket s = clientSocket;
                if (s != null)
                {
                    clientSocket.Blocking = blockingState;
                }
            }

            return isConnected;
        }

        public TimeSpan Timeout
        {
            get;
            set;
        }

        public DateTime ActiveTime
        {
            get;
            set;
        }

        public DateTime LastRequestTime
        {
            get;
            set;
        }

        public DateTime LastResponseTime
        {
            get;
            set;
        }

        #endregion
    }
}
