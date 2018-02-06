using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket;
using Dynamic.Net.Server;
using System.Net;
using Dynamic.Net.Base;
using Dynamic.Core.Runtime;
using System.Threading;
using Dynamic.Net.Session;
using Dynamic.Core.Log;
using Dynamic.Net.Application;
using System.Linq.Expressions;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    /// <summary>
    /// 膜拜万能的WebSocket
    /// 此服务是基于WebSocket协议的第三层封装：WebSocket协议(数据帧传输)-->Dynamic.Net命令协议(Binary)-->ICommand
    /// DynamicBlue 20121106
    /// </summary>
    public class CommandServer
    {
        private ILogger logger = null;

        public String IP { get; private set; }
        public int Port { get; private set; }

        public TimeSpan RequestTimeout { get; set; }

        public event EventHandler<CommandReceivedEventArgs<WSCommandTypeBase>> CommandReceived;
       // public event EventHandler<CommandReceivedEventArgs<WSBinaryCommandType>> BinaryCommandReceived;
        public event EventHandler<CommandSession> ClientClosed;
        public event EventHandler<CommandSession> ClientConnected;
        public event EventHandler ServerOpened;

        private WebSocketApplication application = null;
        protected Dictionary<String, CommandSession> SessionList = null;

        private object lockerObj = new object();

        public bool IsAync { get; set; }

        public String Protocol { get; private set; }
        public Object Context { get; set; }
        public TimeSpan CommandAliveTime { get; set; }

        /// <summary>
        /// 服务所支持的命令列表
        /// </summary>
        public List<ICommand> CommandList { get; private set; }

        public List<ICommandParser> CommandParser { get; private set; }

        protected ICommandParser DefaultCommandParser { get; set; }

        public ISocketSessionFactory SessionFactory { get; protected set; }

        public SocketServerConfig ServerConfig { get; private set; }

        //protected IWebSocketCommandFactory CommandFactory { get; set; }

        //private Dictionary<WebSocketCommandType, IWebSocketCommandFactory> commandFactoryDic = new Dictionary<WebSocketCommandType, IWebSocketCommandFactory>()
        //{
        //    {WebSocketCommandType.Binary, new DefaultWebSocketCommandFactory(WebSocketCommandType.Binary)},
        //    {WebSocketCommandType.Text, new DefaultWebSocketCommandFactory(WebSocketCommandType.Text)}
        //};


        //public CommandServer(string ipAddress, int port, string appName, string serverName, string useProtocol)
        //    : this(ipAddress, port, appName, serverName, useProtocol)
        //{
        //}

        public CommandServer(string ipAddress, int port, string appName, string serverName, string useProtocol)
            : this(ipAddress, port, appName, serverName, useProtocol, new WebSocketSessionFactory())
        {
        }

        /// <summary>
        /// 新建一个CommandServer
        /// </summary>
        /// <param name="ipAddress">监听的IP地址，如果传入空对应于IPAddress.Any</param>
        /// <param name="port">监听端口</param>
        /// <param name="appName">应用名称，表示此服务的名称，对功能无关紧要</param>
        /// <param name="serverName">服务名称，表示服务使用的内部TCP监听服务的名称，对功能无关紧要</param>
        /// <param name="useProtocol">所使用的协议</param>
        /// <param name="sessionFactory">创建内部Session的工厂, 注意：工厂所创建的Session必须是WebSocketSession的子类</param>
        public CommandServer(string ipAddress, int port, string appName, string serverName, string useProtocol, ISocketSessionFactory sessionFactory)
        {
            logger = LoggerManager.GetLogger("CommandServer." + (appName == "" ? "UnName" : appName));
            DefaultCommandParser = new WSBinaryCommandType();
            this.SessionFactory = sessionFactory;

            IP = ipAddress;
            Port = port;
            RequestTimeout = TimeSpan.FromMinutes(2);
            SessionList = new Dictionary<string, CommandSession>();
            Protocol = useProtocol;
            IsAync = true;
            CommandList = new List<ICommand>();

            CommandList.Add(new SetCommandParserRequest());

            CommandParser = new List<ICommandParser>(){
                new WSCommandType(),
                new WSBinaryCommandType()
            };

            CommandAliveTime = TimeSpan.FromMinutes(2);

            IPAddress address = IPAddress.Any;
            if (!String.IsNullOrEmpty(ipAddress))
            {
                address =  IPAddress.Parse(IP);
            }

            application = new WebSocketApplication();
            application.Setup(appName);
            AsyncTcpServer server = new AsyncTcpServer();
            ServerConfig = new SocketServerConfig()
            {
                Address = address,
                AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork,
                MaxConnectionNumber = 1000,
                Name = serverName,
                Port = Port,
                SessionTimeout = (long)TimeSpan.FromMinutes(5).TotalMilliseconds,
                TimeoutType = Dynamic.Net.Session.SessionTimeoutType.Unknown, //不过期
                ServerType = NetServerType.ASyncTcp
            };
            server.Setup(ServerConfig, application, new WebSocketProtocol(), sessionFactory);


            application.AddServer(server);


            application.SwitchingProtocol += new EventHandler<SwitchingProtocolEventArgs>(SwitchingProtocol);

            application.SessionStarted += new EventHandler(SessionStarted);

            application.SessionClosed += new EventHandler(SessionClosed);
            application.HandshakeCompleted += new EventHandler(HandshakeCompleted);

            application.MessageReceived += new EventHandler<MessageReceivedEventArgs>(MessageReceived);
        }


        protected NetApplicationBase Application
        {
            get
            {
                return application;
            }
        }

        protected virtual  void HandshakeCompleted(object sender, EventArgs e)
        {
            WebSocketSessionBase ws = sender as WebSocketSessionBase;
            if (ws != null)
            {

                if (ClientConnected != null)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
                    {
                        CommandSession cs = null;
                        lock (lockerObj)
                        {
                            if (SessionList.ContainsKey(ws.SessionID))
                            {
                                cs = SessionList[ws.SessionID];
                            }
                        }
                        if (cs != null)
                        {
                            ClientConnected(this, cs);
                        }
                    }), null);
                }
             
            }
        }

        protected virtual void SessionClosed(object sender, EventArgs e)
        {
            WebSocketSessionBase ws = sender as WebSocketSessionBase;
            CommandSession client = null;

            lock (lockerObj)
            {
                if (SessionList.ContainsKey(ws.SessionID))
                {
                    client = SessionList[ws.SessionID];
                }
            }

            if (client == null)
                return;

            if (client != null)
            {
                if (ClientClosed != null)
                {
                    ClientClosed(this, client);
                }
            }
            if (ws != null)
            {
                lock (lockerObj)
                {
                    try
                    {
                        SessionList.Remove(ws.SessionID);
                    }
                    catch { }
                }
            }
            
        }

        protected virtual void  MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            e.IsAync = IsAync;
            if (IsAync)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
                {
                    ProcessMessage(o as MessageReceivedEventArgs);
                }), e);
            }
            else
            {
                ProcessMessage(e);
            }
           
        }

        protected virtual void ProcessMessage(MessageReceivedEventArgs e)
        {
            ProcessMessage(e, CommandList);
        }

        public bool ProcessMessage(MessageReceivedEventArgs e, List<ICommand> commandList)
        {
            ICommandParser cmdParser;
            WSCommandTypeBase cmd = GetRequestCommand(e, out cmdParser);
            if (cmd == null)
                return false;

            bool isProcessed = false;

            if (cmd != null)
            {
                // 关于是否触发CommandReceived事件：
                //      当使用带有回调方法的SendCommand发送命令时，接收到应答命令时不会触发CommandReceived事件
                //      当使用SendCommandSync方法发送同步返回的命令时，接收到应答命令时不会触发CommandReceived事件
                //      当接收到的命令能够被内置的CommandList处理时，会触发CommandReceived事件，并且自动设置ReplyCommand为执行完后返回的命令，
                // 同时将事件参数中的IsUnknown属性设置为false。
                //      当接收到的命令不属于上述任一情况时，将触发CommandReceived事件，并将IsUnknown属性设置为true



                CommandSession client = null;
                lock (lockerObj)
                {
                    if (SessionList.ContainsKey(e.Session.SessionID))
                    {
                        client = SessionList[e.Session.SessionID];
                    }
                }

                if (client == null)
                {
                    return false;
                }
                CommandItem<WSCommandTypeBase> ci = null;
                lock (client.SyncLocker)
                {
                    ci = client.Commands.OfType<CommandItem<WSCommandTypeBase>>().FirstOrDefault(x => x.CommandRequest != null && x.CommandRequest.IsPairCommand(cmd));
                }
                CommandReceivedEventArgs<WSCommandTypeBase> args = new CommandReceivedEventArgs<WSCommandTypeBase>(cmd);
                args.IsUnknwon = true;

                // 检查命令是否能被内置命令列表处理
                ICommand exeCommand = commandList.FirstOrDefault(x => x.CanExecute(cmd));
                ICommand replyCommand = null;
                bool isFireReceivedEvent = true;
                if (exeCommand != null)
                {
                    try
                    {
                        replyCommand = exeCommand.Execute(cmd, new ExecuteCommandContext() { CommandSession = client });
                        isProcessed = true;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("执行命令失败：{0} \r\n{1}", cmd.CommandName, ex.ToString());
                    }



                    args.IsUnknwon = false;
                }


                args.Session = client;
                if (ci != null)
                {
                    ci.CommandResponse = cmd;
                    args.RequestCommand = ci.CommandRequest;
                    lock (client.SyncLocker)
                    {
                        if (cmd.IsOver)
                        {
                            client.Commands.Remove(ci);
                        }
                    }
                    if (ci.ResponseCallback != null)
                    {
                        ci.ResponseCallback(ci);
                        isFireReceivedEvent = false;
                    }

                    if (ci.IsSync)
                    {
                        ci.Wait.Set();
                        isFireReceivedEvent = false;
                    }
                }

                if (isFireReceivedEvent)
                {
                    onCommandReceived(args);
                }

                SetReplyCommand(client, replyCommand, e, cmd.RequestID, cmdParser);

            }

            return isProcessed;
        }

        protected WSCommandTypeBase GetRequestCommand(MessageReceivedEventArgs e, out ICommandParser cmdParser)
        {
            WSCommandTypeBase cmd = null;
            cmdParser = CommandParser.FirstOrDefault(c => c.CanRead(e, out cmd));
            if (cmdParser == null)
                return null;

            return cmd;
        }

        protected void SetReplyCommand(CommandSession client, ICommand replayCmd, MessageReceivedEventArgs e,Guid requestGuid, ICommandParser cmdParser)
        {
            if (replayCmd != null)
            {
                WSCommandTypeBase replyCommandType = cmdParser.Create();
                replayCmd.ToCommand(replyCommandType);


                replyCommandType.RequestID = requestGuid;
                cmdParser.SetReplyCommand(e, replyCommandType);
                if (IsAync)
                {
                    //异步需要自己向Session发送消息
                    client.Session.SendMessage(cmdParser.ToBinary(replyCommandType), cmdParser.TransferEncoder);
                }
            }
        }


        //public virtual void AttachSession(WebSocketSessionBase session)
        //{
        //    application.AttachSession(session);
        //    SessionStarted(session, EventArgs.Empty);
        //}

        //public virtual void DettachSession(WebSocketSessionBase session)
        //{
        //    lock (lockerObj)
        //    {
        //        SessionList.Remove(session.SessionID);
        //        application.DettachSession(session);
        //    }
            
        //}

        public virtual void AttachSession(CommandSession commandSession)
        {
            if (commandSession == null)
            {
                logger.Warn("AttachSession 传入Session为空");
                return;
            }
            application.AttachSession(commandSession.Session);
            WebSocketSessionBase ws = commandSession.Session as WebSocketSessionBase;       
            if (ws != null)
            {
                ws.SessionIDChanged += new EventHandler<SessionIDChangedEventArgs>(ws_SessionIDChanged);

                lock (lockerObj)
                {
                    SessionList.Add(ws.SessionID, commandSession);
                }

            }

        }

        public virtual void DettachSession(CommandSession commandSession)
        {
            if (commandSession == null)
                return;
            lock (lockerObj)
            {
                SessionList.Remove(commandSession.Session.SessionID);
                application.DettachSession(commandSession.Session);
            }

        }

        public virtual List<CommandSession> GetSession(Func<CommandSession, bool> condition)
        {
            lock (lockerObj)
            {
                return SessionList.Values.Where(condition).ToList();
            }
        }

        protected virtual void SessionStarted(object sender, EventArgs e)
        {
            WebSocketSessionBase ws = sender as WebSocketSessionBase;
            ws.SessionIDChanged += new EventHandler<SessionIDChangedEventArgs>(ws_SessionIDChanged);
            
            CommandSession session = new CommandSession() { Session = ws };
            if (session.CommandParser == null)
            {
                session.CommandParser = DefaultCommandParser;
            }
            if (ws != null)
            {
                lock (lockerObj)
                {
                    SessionList.Add(ws.SessionID, session);
                }

            }

           
        }

        void ws_SessionIDChanged(object sender, SessionIDChangedEventArgs e)
        {
            lock (lockerObj)
            {
                CommandSession oldSession = SessionList[e.Old];
                SessionList.Remove(e.Old);
                SessionList.Add(e.New, oldSession);
            }
        }

        public CommandSession GetSession(string sessionID)
        {
            lock (lockerObj)
            {
                if (SessionList.ContainsKey(sessionID))
                {
                    return SessionList[sessionID];
                }
            }

            return null;
        }

        protected virtual void SwitchingProtocol(object sender, SwitchingProtocolEventArgs e)
        {
            e.SelectedProtocol = Protocol;
        }

        protected virtual void onCommandReceived(CommandReceivedEventArgs<WSCommandTypeBase> args)
        {
            if (CommandReceived != null)
            {
                CommandReceived(this, args);
            }
        }

        //protected virtual void onCommandReceived(CommandReceivedEventArgs<WSBinaryCommandType> args)
        //{
        //    if (BinaryCommandReceived != null)
        //    {
        //        BinaryCommandReceived(this, args);
        //    }
        //}

        public String Address
        {
            get
            {
                return String.Format("ws://{0}:{1}", IP, Port);
            }
        }

        public bool Start()
        {

            bool isSuccess = application.Start();
            clearRequestCommands();
            bool isRunning = (application.ServerList.First() as AsyncTcpServer).IsRunning;
            if (isRunning)
            {
                if (ServerOpened != null)
                {
                    ServerOpened(this, EventArgs.Empty);
                }
            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
                {
                    Thread.Sleep(2000);
                    if (IsRunning)
                    {
                        if (ServerOpened != null)
                        {
                            ServerOpened(this, EventArgs.Empty);
                        }
                    }
                }), null);
            }

            return isRunning;
        }

        public void Stop()
        {
            application.Stop();
            lock (lockerObj)
            {
                SessionList.Clear();
            }
        }

        public bool IsRunning
        {
            get
            {
                if (application.IsStarted)
                {
                    AsyncTcpServer server = application.ServerList.FirstOrDefault() as AsyncTcpServer;
                    if (server != null)
                    {
                        return server.IsRunning;
                    }
                }
                return false;
            }
        }

        public int SessionCount
        {
            get
            {
                return (SessionList == null ? 0 : SessionList.Count);
            }
        }

        void clearRequestCommands()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(CommandAliveTime.TotalMilliseconds / 2));
                List<CommandSession> unActiveSession = new List<CommandSession>();
                lock (lockerObj)
                {
                    if (SessionList != null && SessionList.Any())
                    {
                        SessionList.All(x =>
                        {
                            if (x.Value != null)
                            {
                                x.Value.ClearRequestCommand(DateTime.Now - CommandAliveTime);
                            }

                            if (x.Value.Session != null && !x.Value.Session.IsHandShake &&
                                (DateTime.Now - x.Value.Session.StartTime).TotalSeconds >= 30)
                            {
                                unActiveSession.Add(x.Value);
                            }
                            
                            return true;
                        });
                    }


                    unActiveSession.All(s =>
                    {
                        logger.Info("关闭非法会话：{0}", s.Session.SessionID);
                        s.Session.Close();
                        return true;
                    });
                }
                if (IsRunning)
                {
                    clearRequestCommands();
                }
            }), null);
        }


        //public void SendCommand(WebSocketSessionBase session, WSCommandType command)
        //{

        //    string commandText = SerializationUtility.ToXmlString(command);
        //    session.SendMessage(commandText);
        //}

        public void Broadcast(ICommand command)
        {
            if (command == null)
                return;

            if (SessionList == null || SessionList.Count == 0)
                return;

            lock (Application.SyncLocker)
            {
                SessionList.All(session =>
                {
                    if (session.Value != null)
                    {
                        try
                        {
                            session.Value.SendCommand(command);
                        }
                        catch { }
                    }
                    return true;
                });
            }

            //command.RequestID = Guid.NewGuid();
            //command.OccurTime = DateTime.Now;
            //string commandText = SerializationUtility.ToXmlString(command);
            //application.Broadcast(commandText);
        }

        public void Broadcast(ICommand command, Func<WebSocketSessionBase, bool> checkFunc)
        {
            if (command == null)
                return;

            if (SessionList == null || SessionList.Count == 0)
                return;

            lock (Application.SyncLocker)
            {
                SessionList.Where(x =>
                {
                    if (x.Value == null)
                        return false;
                    return checkFunc(x.Value.Session);
                }).All(session =>
                {
                    if (session.Value != null)
                    {
                        try
                        {
                            session.Value.SendCommand(command);
                        }
                        catch { }
                    }
                    return true;
                });
            }
        }

        public void Broadcast(WSCommandTypeBase command)
        {
            if (command == null)
                return;

            if (SessionList == null || SessionList.Count == 0)
                return;

            lock (Application.SyncLocker)
            {
                SessionList.All(session =>
                {
                    if (session.Value != null)
                    {
                        try
                        {
                            session.Value.SendCommand(command);
                        }
                        catch { }
                    }
                    return true;
                });
            }
        }

        public void Broadcast(WSCommandTypeBase command, Func<WebSocketSessionBase, bool> checkFunc)
        {
            if (command == null)
                return;

            if (SessionList == null || SessionList.Count == 0)
                return;

            lock (Application.SyncLocker)
            {
                SessionList.Where(x =>
                {
                    if (x.Value == null)
                        return false;
                    return checkFunc(x.Value.Session);
                }).All(session =>
                {
                    if (session.Value != null)
                    {
                        try
                        {
                            session.Value.SendCommand(command);
                        }
                        catch { }
                    }
                    return true;
                });
            }
        }



        //public void Broadcast(WSCommandType command)
        //{
        //    if (command == null)
        //        return;

        //    command.RequestID = Guid.NewGuid();
        //    command.OccurTime = DateTime.Now;
        //    string commandText = SerializationUtility.ToXmlString(command);
        //    application.Broadcast(commandText);
        //}

        //public void Broadcast(WSCommandType command, Func<WebSocketSession, bool> checkFunc)
        //{
        //    if (command == null)
        //        return;

        //    command.RequestID = Guid.NewGuid();
        //    command.OccurTime = DateTime.Now;
        //    string commandText = SerializationUtility.ToXmlString(command);
        //    application.Broadcast(commandText, checkFunc);
        //}

        //public void Broadcast(WSBinaryCommandType command)
        //{
        //    if (command == null)
        //    {
        //        return;
        //    }
        //    command.RequestID = Guid.NewGuid();
        //    command.OccurTime = DateTime.Now;
        //    byte[] commandData = BinaryCommandTypeSerializer.ToBinary(command);
        //    application.Broadcast(commandData);
        //}

        //public void Broadcast(WSBinaryCommandType command, Func<WebSocketSession,bool> checkFunc)
        //{
        //    if (command == null)
        //    {
        //        return;
        //    }
        //    command.RequestID = Guid.NewGuid();
        //    command.OccurTime = DateTime.Now;
        //    byte[] commandData = BinaryCommandTypeSerializer.ToBinary(command);
        //    application.Broadcast(commandData, checkFunc);
        //}

        public void SendCommand(CommandSession session, ICommand command)
        {
            session.SendCommand(command);
        }

        public virtual void SendCommand(CommandSession session, ICommand command, CommandResponse callback)
        {
            if (session == null)
                return;
            session.SendCommand(command, callback);
        }

        public virtual CommandItem<WSCommandTypeBase> SendCommandSync(CommandSession session, ICommand command)
        {
            if (session == null)
                return null;

            return session.SendCommandSync(command, RequestTimeout);
        }



        public void SendCommand(CommandSession session, WSCommandTypeBase command)
        {
            SendCommand(session, command, null);
        }

        public virtual void SendCommand(CommandSession session, WSCommandTypeBase command, CommandResponse callback)
        {
            if (session == null)
                return;
            session.SendCommand(command, callback);
        }

        public virtual CommandItem<WSCommandTypeBase> SendCommandSync(CommandSession session, WSCommandTypeBase command)
        {
            if (session == null)
                return null;

           return  session.SendCommandSync(command, RequestTimeout);
        }

        //public void SendCommand(CommandSession session, WSBinaryCommandType command)
        //{
        //    SendCommand(session, command, null);
        //}

        //public virtual void SendCommand(CommandSession session, WSBinaryCommandType command, CommandResponse callback)
        //{
        //    if (session == null)
        //        return;
        //    session.SendCommand(command, callback);
        //}

        //public virtual CommandItem<WSBinaryCommandType> SendCommandSync(CommandSession session, WSBinaryCommandType command)
        //{
        //    if (session == null)
        //        return null;

        //    return session.SendCommandSync(command, RequestTimeout);
        //}
    }
}
