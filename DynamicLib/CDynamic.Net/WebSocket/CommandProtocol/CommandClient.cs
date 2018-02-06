using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket;
using Dynamic.Core.Runtime;
using System.Net;
using Dynamic.Net.WebSocket.CommandProtocol;
using System.Text.RegularExpressions;
using System.Threading;
using Dynamic.Core.Log;
using System.Diagnostics;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    /// <summary>
    /// 客户端
    /// </summary>
    public  class CommandClient
    {
        private String ip;
        public String IP
        {
            get
            {
                return ip;
            }
            set
            {
                if (ip != value)
                {
                    if (IsConnected)
                    {
                        throw new NotSupportedException("已连接状态下无法修改目标IP，请先关闭连接!");
                    }
                    else
                    {
                        ip = value;
                    }
                }
            }
        }

        private int port;
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                if (port != value)
                {
                    if (IsConnected)
                    {
                        throw new NotSupportedException("已连接状态下无法修改目标端口，请先关闭连接!");
                    }
                    else
                    {
                        port = value;
                    }
                }
            }
        }

        public int MaxRetryCount { get; set; }

        private ILogger logger = null;

        private ILogger traceLogger = LoggerManager.GetLogger("CommandClientTrace");

        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler Closed;
        public event EventHandler<CommandReceivedEventArgs<WSCommandTypeBase>> CommandReceived;

        public TimeSpan RequestTimeout { get; set; }

        protected object lockerObj = new object();

        private WebSocketClient client = null;

        private int retryCount = 0;

        public bool IsConnected { get; protected set; }


        protected bool IsConnecting { get; set; }

        public CommandSession Session { get; protected set; }

        public Object Context { get; set; }

        public TimeSpan CommandAliveTime { get; set; }

        public bool IsAync { get; set; }

        public List<ICommand> CommandList { get; private set; }

        public List<ICommandParser> CommandParser { get; private set; }

        /// <summary>
        /// 发送命令时使用的命令解析器
        /// </summary>
        protected ICommandParser SessionCommandParser { get; set; }


        public CommandClient(string ipAddress, int port, string host, string protocol)
            : this(ipAddress, port, host, protocol, new WebSocketClientSessionFactory())
        {
        }

        public CommandClient(string ipAddress, int port, string host, string protocol, IWebSocketClientSessionFactory sessionFactory)
            : this(ipAddress, port, host, protocol, sessionFactory, new WSBinaryCommandType())
        {
        }

        public CommandClient(string ipAddress, int port, string host, string protocol, IWebSocketClientSessionFactory sessionFactory, ICommandParser commandParser)
        {
            logger = LoggerManager.GetLogger(String.Format("CommandClient_{0}_{1}", ipAddress, port));
            SessionCommandParser = commandParser;
            IsConnecting = false;

            IP = ipAddress;
            Port = port;
            RequestTimeout = TimeSpan.FromMinutes(2);
            MaxRetryCount = 3;
            IsAync = true;
            Session = new CommandSession();
            Session.CommandParser = commandParser;
            IsConnected = false;
            CommandAliveTime = TimeSpan.FromMinutes(2);
            CommandList = new List<ICommand>();

            CommandList.Add(new SetCommandParserResponse());

            CommandParser = new List<ICommandParser>()
            {
                new WSCommandType(),
                new WSBinaryCommandType()
            };

            client = new WebSocketClient(host, "", protocol, sessionFactory);
            
            client.Connected += new EventHandler<WebSocketConnectedEventArgs>(ClientConnected);
            client.Closed += new EventHandler(ClientClosed);
            client.MessageReceived += new EventHandler<MessageReceivedEventArgs>(MessageReceived);
            
        }



        protected virtual void MessageReceived(object sender, MessageReceivedEventArgs e)
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

                CommandItem<WSCommandTypeBase> ci = null;
                lock (Session.SyncLocker)
                {
                    ci = Session.Commands.OfType<CommandItem<WSCommandTypeBase>>().FirstOrDefault(x => x.CommandRequest != null && x.CommandRequest.IsPairCommand(cmd));
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
                        replyCommand = exeCommand.Execute(cmd, new ExecuteCommandContext() { CommandSession = Session });
                        isProcessed = true;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("执行命令失败：{0} \r\n{1}", cmd.CommandName, ex.ToString());
                    }


                    args.IsUnknwon = false;
                }


                args.Session = Session;
                if (ci != null)
                {
                    ci.CommandResponse = cmd;
                    args.RequestCommand = ci.CommandRequest;
                    lock (Session.SyncLocker)
                    {
                        if (cmd.IsOver)
                        {
                            Session.Commands.Remove(ci);
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

                SetReplyCommand(Session, replyCommand, e, cmd.RequestID, cmdParser);
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

        protected void SetReplyCommand(CommandSession client, ICommand replayCmd, MessageReceivedEventArgs e, Guid requestGuid, ICommandParser cmdParser)
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

        protected virtual void ClientClosed(object sender, EventArgs e)
        {
  //          IsConnecting = false;
            //取消掉所有等待中的命令
            if (Session.Commands != null && Session.Commands.Any())
            {
                Session.Commands.All(cmd =>
                {
                    if (cmd != null && cmd.IsSync && cmd.Wait != null)
                    {
                        cmd.Wait.Set();
                    }

                    return true;
                });
            }

            if (Closed != null && IsConnected && !IsConnecting)
            {
                IsConnected = false;

                Closed(this, EventArgs.Empty);
            }
        }

        protected virtual  void ClientConnected(object sender, WebSocketConnectedEventArgs e)
        {
            traceLogger.Trace("Socket连接完成通知：" + e.IsSuccess.ToString());
            if (!e.IsSuccess)
            {
                if (retryCount >= MaxRetryCount)
                {
                    //重试失败
                    traceLogger.Trace("重试失败");
                    onConnected(false);

                    retryCount = 0;
                }
                else
                {
                    retryCount++;
                    traceLogger.Trace("重试：{0}", retryCount);
                    Connect(true);
                   
                }
            }
            else
            {
                Session.Session = client.Session;
                if (Session.Session is WebSocketClientSession)
                {
                    WebSocketClientSession clientSession = Session.Session as WebSocketClientSession;
                    clientSession.RemoteIP = IP;
                    clientSession.RemotePort = Port;
                }
               
                // 一定不要在Socket数据接收主线程中使用同步发送命令的方法
                ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
                {
                    try
                    {
                        traceLogger.Trace("开始OnBeforeConnected");
                        IsConnected = OnBeforeConnected();
                        traceLogger.Trace("连接状态：{0}", IsConnected);
                    }
                    catch(Exception ex)
                    {
                        logger.Error("初始化连接出错：{0}" , ex.ToString());
                    }

                    if (!IsConnected)
                    {
                        traceLogger.Trace("连接失败，关闭基础连接");
                        Close();
                    }

                    onConnected(IsConnected);
                }), null);


                clearRequestCommands();
            }
        }

        protected virtual void onCommandReceived(CommandReceivedEventArgs<WSCommandTypeBase> args)
        {
            if (CommandReceived != null)
            {
                CommandReceived(this, args);
            }
        }


        //在子类中实现此方法，用于插入触发Connected事件之前的特殊处理
        protected virtual bool OnBeforeConnected()
        {
            bool isSuccess = false;
            Debug.WriteLine("Client Socket Status：" + Session.Session.SessionID + " " + Session.Session.ClientSocket.Connected.ToString());
            CommandItem<WSCommandTypeBase> response = Session.SendCommandSync(new SetCommandParserRequest(SessionCommandParser.ParserID), RequestTimeout);
            if (response != null && response.CommandResponse != null)
            {
                SetCommandParserResponse cmdParserResponse = new SetCommandParserResponse(response.CommandResponse);
                if (cmdParserResponse.IsSuccess)
                {
                    isSuccess = true;
                }
            }

            return isSuccess;
          //  return true;
        }

        protected void onConnected(bool isSuccess)
        {
            lock (lockerObj)
            {
                IsConnecting = false;
            }
            traceLogger.Trace("触发连接事件");
            if (Connected != null)
            {
                Connected(this, new ConnectedEventArgs() { Client = this, IsSuccess = isSuccess });
            }
        }


        public void Close()
        {
            traceLogger.Trace("关闭");

            
           // IsConnecting = false;
            client.Close();
        }


        public virtual void Connect()
        {
            traceLogger.Trace("开始连接");
            Connect(false);
        }

        public virtual void Connect(bool isRetry)
        {
            
            lock (lockerObj)
            {
                if (IsConnecting && !isRetry)
                {
                    traceLogger.Trace("连接中，忽略连接请求");
                    return;
                }
                IsConnecting = true;
            }

           


            Regex regex = new Regex("[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}");
            if (regex.IsMatch(IP))
            {
                client.Connect(IPAddress.Parse(IP), Port);
            }
            else
            {
                client.Connect(IP, Port);
            }

           
        }

        void clearRequestCommands()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(CommandAliveTime.TotalMilliseconds / 2));
                Session.ClearRequestCommand(DateTime.Now - CommandAliveTime);
                if (IsConnected)
                {
                    clearRequestCommands();
                }
            }), null);
        }

        public void SendCommand(ICommand command)
        {
            SendCommand(command, null);
        }

        public virtual void SendCommand(ICommand command, CommandResponse callback)
        {

            Session.SendCommand(command, callback);
        }


        public virtual void SendCommand<T>(ICommand command, CommandCallback<T> callback)
        where T : CommandBase, new()
        {
            Session.SendCommand(command, callback);
        }


        public virtual CommandItem<WSCommandTypeBase> SendCommandSync(ICommand command)
        {
            return Session.SendCommandSync(command, RequestTimeout);
        }


        public virtual T SendCommandSync<T>(ICommand command) where T :CommandBase, new ()
        {
            CommandItem<WSCommandTypeBase> response = SendCommandSync(command);
            if (response != null && response.CommandResponse != null)
            {
                CommandBase responseCommand = new T();
                responseCommand.LoadCommand(response.CommandResponse);
                return responseCommand as T;
            }

            return null;
        }
    }
}
