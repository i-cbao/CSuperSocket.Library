using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Dynamic.Net.WebSocket;
using Dynamic.Core.Log;
using System.Threading;
using Dynamic.Net.Session;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketClient
    {
        private class ConnectItem
        {
            public string HostName { get; set; }
            public int Port { get; set; }
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<WebSocketConnectedEventArgs> Connected;
        public event EventHandler Closed;
        private static ILogger logger = LoggerManager.GetLogger("WebSocketClient");
        private ILogger traceLogger = LoggerManager.GetLogger("CommandClientTrace");

        public Socket client = null;
        WebSocketProtocol protocol = null;
        WebSocketSessionBase session = null;

        public String Host { get; private set; }
        public String Origin { get; private set; }
        public String Protocols { get; private set; }
        public WebSocketSessionBase Session
        {
            get
            {
                return session;
            }
        }

        public bool IsConnected { get; private set; }
        private bool isConnecting = false; //是否正在连接
        private object lockerObj = new object();

        public IPAddress IPAddress { get; private set; }



        public string Url { get; set; }

        private IWebSocketClientSessionFactory sessionFactory = null;

        public WebSocketClient(string host, string origin, string protocols)
            :this(host, origin, protocols, new WebSocketClientSessionFactory() )
        {
        }

        /// <summary>
        /// 创建一个WebSocket连接客户端
        /// </summary>
        /// <param name="host">主机名称</param>
        /// <param name="origin">发起源地址</param>
        /// <param name="protocols">使用协议</param>
        /// <param name="sessionFactory">会话工厂，你可以从WebSocketSessionBase继承，实现自己的会话处理</param>
        public WebSocketClient(string host, string origin, string protocols, IWebSocketClientSessionFactory sessionFactory)
        {
            protocol = new WebSocketProtocol();

            this.Host = host;
            this.Origin = origin;
            this.Protocols = protocols;

            this.sessionFactory = sessionFactory;
        }




        public bool Connect(IPAddress address, int port)
        {
            lock (lockerObj)
            {
                if (isConnecting)
                {
                    traceLogger.Trace("WebSocketClient连接中，忽略连接请求");
                    return false;
                }

                isConnecting = true;
            }
            try
            {
                if (client != null)
                {
                    client.Close();
                    client = null;
                }
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                SocketAsyncEventArgs asyncEvent = new SocketAsyncEventArgs();
                asyncEvent.RemoteEndPoint = new IPEndPoint(address, port);
                asyncEvent.AcceptSocket = client;

                asyncEvent.Completed += new EventHandler<SocketAsyncEventArgs>(asyncEvent_Completed);
                bool isAsync = client.ConnectAsync(asyncEvent);
                if (!isAsync)
                {
                    asyncEvent_Completed(client, asyncEvent);
                }

            }
            catch (Exception e)
            {
                logger.Error("连接异常：{0} : {1}   {2}",address.ToString(), port, e.ToString());
                isConnecting = false;
                if (Connected != null)
                {

                    Connected(this, new WebSocketConnectedEventArgs(false, this));
                }
                return false;
            }

            return true;
        }

        public bool Connect(string hostname, int port)
        {
            lock (lockerObj)
            {
                if (isConnecting)
                    return false;

                isConnecting = true;
            }

            ConnectItem ci = new ConnectItem() { HostName = hostname, Port = port };
            ThreadPool.QueueUserWorkItem(new WaitCallback((p) =>
            {
                
                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                bool isConnected = false;
                IPAddress[] hostAddresses = null;
                Socket socket = null;
                Socket socket2 = null;

                try
                {
                    hostAddresses = Dns.GetHostAddresses(hostname);
                    socket = null;
                    socket2 = null;

                    if (this.client == null)
                    {
                        if (Socket.SupportsIPv4)
                        {
                            socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        }
                        if (Socket.OSSupportsIPv6)
                        {
                            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                        }
                    }
                    foreach (IPAddress address in hostAddresses)
                    {
                        try
                        {
                            if (this.client == null)
                            {
                                if ((address.AddressFamily == AddressFamily.InterNetwork) && (socket2 != null))
                                {
                                    socket2.Connect(address, port);
                                    isConnected = true;
                                    this.client = socket2;
                                    if (socket != null)
                                    {
                                        socket.Close();
                                    }
                                }
                                else if (socket != null)
                                {
                                    socket.Connect(address, port);
                                    isConnected = true;
                                    this.client = socket;
                                    if (socket2 != null)
                                    {
                                        socket2.Close();
                                    }
                                }
                                IPAddress = address;

                            }

                        }
                        catch
                        {
                            isConnected = false;
                        }
                    }
                }
                catch
                {
                    isConnecting = false;
                }
                finally
                {
                    if (!isConnected)
                    {
                        if (socket != null)
                        {
                            socket.Close();
                        }
                        if (socket2 != null)
                        {
                            socket2.Close();
                        }

                        lock (lockerObj)
                        {
                            isConnecting = false;
                        }
                    }


                }

                if (isConnected)
                {

                    sendHandshakeCommand(client);



                }
                else
                {
                    if (Connected != null)
                    {

                        Connected(this, new WebSocketConnectedEventArgs(false, this));
                    }
                }
            }), ci);

            return true;
        }

        public void SendMessage(string message)
        {
            
            session.SendMessage(message);
        }

        void asyncEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            traceLogger.Trace("WebSocketClient连接完成");
            if (e.AcceptSocket.Connected)
            {
                sendHandshakeCommand(e.AcceptSocket);
            }
            else
            {
                lock (lockerObj)
                {
                    isConnecting = false;
                }
                if (Connected != null)
                {

                    Connected(this, new WebSocketConnectedEventArgs(false, this));
                }
            }
           
            
        }

        void sendHandshakeCommand(Socket socket)
        {
            if (socket.Connected)
            {
                try
                {
                    string url = this.Url;
                    if (String.IsNullOrEmpty(url))
                    {
                        url = "/chat";
                    }
                    string header = String.Format(@"GET " + url + @" HTTP/1.1
Host: {0}
Upgrade: websocket
Connection: Upgrade
Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==
Origin: {1}
Sec-WebSocket-Protocol: {2}
Sec-WebSocket-Version: 13

", Host, Origin, Protocols);

                    session = sessionFactory.CreateSession(this, protocol, client);
                    session.SubProtocol = Protocols;


                    session.MessageReceived += new EventHandler<MessageReceivedEventArgs>(session_MessageReceived);
                    session.SessionClosed += new EventHandler(session_SessionClosed);
                    session.HandshakeCompleted += new EventHandler(session_HandshakeCompleted);

                    session.Start();

                    socket.Send(Encoding.UTF8.GetBytes(header));

                }
                catch
                {
                    lock (lockerObj)
                    {
                        isConnecting = false;
                    }
                    if (Connected != null)
                    {
                        Connected(this, new WebSocketConnectedEventArgs(false, this));
                    }
                }
            }
        }

        void session_HandshakeCompleted(object sender, EventArgs e)
        {
            lock (lockerObj)
            {
                isConnecting = false;
                IsConnected = true;   
            }
            traceLogger.Trace("握手成功");

            if (Connected != null)
            {
                Connected(this, new WebSocketConnectedEventArgs(IsConnected, this));
            }

        }

        void session_SessionClosed(object sender, EventArgs e)
        {
            if (client != null)
            {
               
                client.Close();
                client = null;
                isConnecting = false;
            }

            if (Closed != null && IsConnected)
            {
                IsConnected = false;
                Closed(this, EventArgs.Empty);
            }
            else if (!IsConnected)
            {
                traceLogger.Trace("在尚未握手之前服务端即断开连接时，发送连接失败事件");
                if (Connected != null)
                {
                    Connected(this, new WebSocketConnectedEventArgs(false, this));
                }
            }
        }

        void session_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            EventHandler<MessageReceivedEventArgs> handler = MessageReceived;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

       

        public void Close()
        {
            isConnecting = false;
            if (client != null && client.Connected)
            {
                client.Close();
                client = null;
            }

            if (session != null)
            {
                session.Close();
            }

            IsConnected = false;
        }
    }
}
