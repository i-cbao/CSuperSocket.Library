using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Session;
using System.Net.Sockets;
using Dynamic.Core.Log;
using Dynamic.Net.WebSocket.Command;
using Dynamic.Net.Base;
using Dynamic.Net.WebSocket.Frames;
using System.Diagnostics;

namespace Dynamic.Net.WebSocket
{
    public abstract class WebSocketSessionBase : AsyncTcpSession
    {

        class SendDataItem
        {
           // public long SendID { get; set; }
            public Byte[] Data { get; set; }
            public int Start { get; set; }
            public int Count { get; set; }
        }

        protected ILogger logger = null;

        public string SubProtocol { get; set; }

        public string ProtocolVersion { get; set; }

        public string Url { get; set; }

        internal event EventHandler<MessageReceivedEventArgs> MessageReceived;

        internal event EventHandler<SwitchingProtocolEventArgs> SwitchingProtocol;

        internal event EventHandler<SessionIDChangedEventArgs> SessionIDChanged;


        internal event EventHandler HandshakeCompleted;

        public bool IsHandShake { get; protected set; }

        public FrameStreamReader FrameReader = null;



        public TimeSpan ReceiveTimeout { get; set; }

        public object objLock = new object();

        protected bool sendOverClosed = false;

        private Queue<SendDataItem> SendList = new Queue<SendDataItem>();




        /// <summary>
        /// 正在发送的消息队列数
        /// </summary>
        public int SendingMessages { get { return SendList.Count; } }


        private bool isSending = false;

        public WebSocketSessionBase(INetApplication application, INetProtocol protocol, INetServer server, Socket clientSocket, ILogger logger)
            : base(application, protocol, server, clientSocket)
        {
            IsHandShake = false;
            ReceiveTimeout = TimeSpan.FromMinutes(2);
            this.logger = logger;
        }

        internal virtual void OnMessageReceived(MessageReceivedEventArgs args)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, args);
                logger.Trace("ReceivedMessage：{0} {1} {2}\r\n{3}", SessionID, EndPoint, args.ContentType, args.ToString());
            }
        }

        internal virtual void OnSwitchingProtocol(SwitchingProtocolEventArgs args)
        {
            if (SwitchingProtocol != null)
            {
                SwitchingProtocol(this, args);
            }
        }

        internal virtual void OnHandshakeCompleted()
        {
            if (HandshakeCompleted != null)
            {
                HandshakeCompleted(this, EventArgs.Empty);
            }
        }

        public override bool IsTimeout()
        {
            if (FrameReader != null && FrameReader.DataReadTime != DateTime.MinValue)
            {
                if ((DateTime.Now - FrameReader.DataReadTime) >= ReceiveTimeout)
                {
                    return true;
                }
            }
            

            return base.IsTimeout();
        }

        public override bool WriteBytes(byte[] data, int start, int count)
        {
            LastResponseTime = DateTime.Now;
            ActiveTime = DateTime.Now;

           // Debug.WriteLine("Client Socket Status(WriteBytes)：" + SessionID + " " + ClientSocket.Connected.ToString());
            if (!isClosed && clientSocket != null)
            {
                try
                {
                    lock (objLock)
                    {
                        // if ( isSending)
                        //  {
                        SendList.Enqueue(new SendDataItem() { Data = data, Start = start, Count = count });
                        //  }

                    }

                    while (SendList.Any())
                    {
                        SendDataItem di = null;
                        lock (objLock)
                        {
                            di = SendList.Dequeue();
                            isSending = true;
                        }

                        //  InnerStream.Write(di.Data, di.Start, di.Count);
                        InnerStream.BeginWrite(di.Data, di.Start, di.Count, new AsyncCallback((asyncResult) =>
                        {
                            try
                            {
                                SendedBytes += di.Count;
                                InnerStream.EndWrite(asyncResult);
                            }
                            catch
                            {
                                Debug.WriteLine("发送数据时异常(WriteBytes)，Offline");
                                Offline();
                            }
                            lock (objLock)
                            {
                                isSending = false;
                            }
                        }), count);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("WriteBytes Error：" + e.Message);
                }

                return true;
            }
            else
            {
                return false;
            }
           
        }

        public virtual void SetSession(string sessionID)
        {
            if (SessionID != sessionID)
            {
                string old = SessionID;
                SessionID = sessionID;
                if (SessionIDChanged != null)
                {
                    SessionIDChanged(this, new SessionIDChangedEventArgs(old, sessionID));
                }
            }
        }

        public bool SendMessage(string content)
        {
            FrameCommandBase command = WebSocketCommandFactory.CreateCommand(content);
            Protocol.WriteCommand(command as INetCommand, this);
            logger.Trace("SendCommand：{0} {1}\r\n{2}", SessionID, EndPoint, command);
            //lock (objLock)
            //{
            //    SendingMessages++;
            //}
            return true;
        }

        public bool SendMessage(Byte[] data)
        {
            FrameCommandBase command = WebSocketCommandFactory.CreateCommand(data);

            if (!IsConnected())
            {
                Debug.WriteLine("连接已断开，无法发送命令");
                return false;
            }

            Protocol.WriteCommand(command as INetCommand, this);

            //if (isClosed || (clientSocket != null && !clientSocket.Connected))
            //{
            //    logger.Warn("连接已断开，命令未正确写入");
            //    return false;
            //}

            logger.Trace("SendCommand：{0} {1}\r\n{2}", SessionID, EndPoint, command);
            //lock (objLock)
            //{
            //    SendingMessages++;
            //}
            return true;
        }

        public bool SendMessage(Byte[] data, MessageContentType contentType)
        {
            int opcodes = 0;
            if (contentType == MessageContentType.Binary)
            {
                opcodes = Opcodes.BinaryFrame;
            }
            else if (contentType == MessageContentType.Text)
            {
                opcodes = Opcodes.TextFrame;
            }
            else
            {
                logger.Warn("不支持的命令格式：" + contentType.ToString());
                return false;
            }
            if (!IsConnected())
            {
                Debug.WriteLine("连接已断开，无法发送命令");
                return false;
            }

            FrameCommandBase command = WebSocketCommandFactory.CreateCommand(data, opcodes);
            //Debug.WriteLine("Client Socket Status(SendMessage)：" + SessionID + " " + ClientSocket.Connected.ToString());
            Protocol.WriteCommand(command as INetCommand, this);
            //if (isClosed || (clientSocket != null && !clientSocket.Connected))
            //{
            //    logger.Warn("连接已断开，命令未正确写入");
            //    return false;
            //}

            logger.Trace("SendCommand：{0} {1}\r\n{2}", SessionID, EndPoint, command);
            //lock (objLock)
            //{
            //    SendingMessages++;
            //}
            return true;
        }
    }
}
