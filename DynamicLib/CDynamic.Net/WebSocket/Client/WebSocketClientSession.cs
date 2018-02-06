using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Session;
using Dynamic.Core.Log;
using Dynamic.Net.Base;
using System.Net.Sockets;
using Dynamic.Net.WebSocket.Command;
using Dynamic.Net.WebSocket.Frames;
using Dynamic.Net.WebSocket;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketClientSession : WebSocketSessionBase
    {
        public String RemoteIP { get; set; }

        public int RemotePort { get; set; }


        public WebSocketClient Client { get; private set; }
        public WebSocketClientSession(WebSocketClient client, INetProtocol protocol, Socket clientSocket)
            : base(null, protocol, null, clientSocket, LoggerManager.GetLogger("WebSocketClientSession"))
        {
            FrameReader = new FrameStreamReader(frameStream);

            Client = client;
        }

        protected override void AsyncEventArgs_Completed(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            //if (e.BytesTransferred <= 0)
            //{
            //    Offline();
            //    return;
            //}

            if (e.SocketError != SocketError.Success)
            {
                Debug.WriteLine(String.Format("接收数据时发生异常(Client)：{0} {1}", e.SocketError, e.RemoteEndPoint));
                Offline();
                logger.Error("接收数据时发生异常：{0} {1}", e.SocketError, e.RemoteEndPoint);
                return;
            }

            if (e.BytesTransferred == 0)
            {
                Close();
                return;
            }

            ReceivedBytes += e.BytesTransferred - e.Offset; 

            //logger.Trace("接收：{0} 忽略：{1} FramePayload：{2}", e.BytesTransferred, noCheckCount, FrameReader.FramePayloadLength);
            //if (noCheckCount == 0 && FrameReader.FramePayloadLength == 0)
            //{
            //    StringBuilder sb = new StringBuilder();
            //    for (int i = e.Offset; i < e.Offset + e.BytesTransferred; i++)
            //    {
            //        sb.Append(e.Buffer[i].ToString("x")).Append(" ");
            //    }
            //    logger.Trace("数据：{0}", sb.ToString()); 
            //}

            for (int i = e.Offset; i < e.Offset + e.BytesTransferred; i++)
            {
                try
                {
                    frameStream.WriteByte(e.Buffer[i]);
                    frameStream.Flush();
                    if (isNoCheck && noCheckCount > 0)
                    {
                        noCheckCount--;
                        continue;
                    }
                    else if (isNoCheck)
                    {
                        isNoCheck = false;
                        noCheckCount = 0;
                    }

                    if (!IsHandShake && Protocol.IsFrameEnd(frameStream))
                    {
                        frameStream.Position = 0;
                        StreamReader sr = new StreamReader(frameStream);
                        string header = sr.ReadToEnd();
                        Regex regex = new Regex("\r\n");
                        string[] headers = regex.Split(header);

                        if (header != null)
                        {
                            string[] first = header.Split(' ');
                            if (first.Length > 2 && first[1] == "101")
                            {
                                IsHandShake = true;
                                OnHandshakeCompleted();
                            }
                        }


                        frameStream.Position = 0;
                        frameStream.SetLength(0);



                    }
                    else if (IsHandShake)
                    {
                        bool isSuccess = FrameReader.ProcessFrame(this);
                        if (!isSuccess)
                        {
                            //HttpCodeResponseCommand cmd = new HttpCodeResponseCommand();
                            //cmd.SetHeader(WebSocketHeader.HttpCode ,"1002");
                            //sendOverClosed = true;
                            //Protocol.WriteCommand(cmd, this);
                            frameStream.Position = 0;
                            frameStream.SetLength(0);
                            logger.Error("接收数据帧发生异常：{0} {1}", SessionID, EndPoint);
                        }
                        else
                        {
                            //
                            if (FrameReader.IsCompleted)
                            {
                                frameStream.Position = 0;
                                frameStream.SetLength(0);
                                logger.Debug("收到数据帧：{0} {1} {2}", SessionID, EndPoint, FrameReader.ToString());
                                FrameCommandBase command = WebSocketCommandFactory.CreateCommand(FrameReader);
                                if (command != null)
                                {
                                    command = command.Execute(this) as FrameCommandBase;
                                    if (command != null)
                                    {
                                        Protocol.WriteCommand(command, this);
                                    }
                                }
                                if (!this.isClosed)
                                {
                                    FrameReader = new FrameStreamReader(frameStream);
                                }
                            }
                            else if (FrameReader.IsContinue)
                            {
                                frameStream.Position = 0;
                                frameStream.SetLength(0);
                                FrameReader.Reset();
                                logger.Debug("收到持续数据帧：{0} {1} {2}", SessionID, EndPoint, FrameReader.ToString());
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    //此异常发生的可能情况是：处理接收数据的过程中，基础连接被关闭
                    //Debug.WriteLine("Client Socket Status(Received)：" + SessionID + " " + ClientSocket.Connected.ToString() + " " + ex.Message);
                    break;
                }
            }



            if (!isClosed)
            {
                Protocol.TryGetCommand(this);
            }
        }

     
    }
}
