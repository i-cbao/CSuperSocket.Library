using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Session;
using System.Net.Sockets;
using Dynamic.Net.Base;
using Dynamic.Net.WebSocket.Frames;
using Dynamic.Net.WebSocket.Command;
using Dynamic.Core.Log;
using System.Diagnostics;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketSession : WebSocketSessionBase
    {
       // private static ILogger logger = null;


        public WebSocketSession(INetApplication application, INetProtocol protocol, INetServer server, Socket clientSocket)
            : base(application, protocol, server, clientSocket, LoggerManager.GetLogger("WebSocketSession"))
        {
        }

        protected override void AsyncEventArgs_Completed(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            //if (e.BytesTransferred <= 0)
            //{
            //    Close();
            //    return;
            //}

            if (e.SocketError != SocketError.Success)
            {
                Debug.WriteLine(String.Format("接收数据时发生异常：{0} {1}", e.SocketError, e.RemoteEndPoint));
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
                        HandshakeRequestCommand command = Protocol.GetCommand(this, frameStream) as HandshakeRequestCommand;
                        HandshakeResponseCommand responseCmd = null;

                        if (command != null)
                        {
                            ProtocolVersion = command.GetHeader(WebSocketHeader.SecWebSocketAccept) ?? "";
                            this.Url = command.GetHeader(WebSocketHeader.Url) ?? "";
                            frameStream.Position = 0;
                            frameStream.SetLength(0);
                            responseCmd = command.Execute(this) as HandshakeResponseCommand;
                            if (responseCmd != null && !isClosed)
                            {
                                FrameReader = new FrameStreamReader(frameStream);
                            }
                        }

                        if (responseCmd != null)
                        {
                            Protocol.WriteCommand(responseCmd, this);
                            IsHandShake = true;
                            OnHandshakeCompleted();
                            logger.Debug("客户端握手成功：{0} {1}", SessionID, EndPoint);
                        }
                        else
                        {
                            logger.Warn("客户端握手失败：{0} {1}", SessionID, EndPoint);
                            this.Close();
                        }

                    }
                    else if (IsHandShake)
                    {
                        bool isSuccess = FrameReader.ProcessFrame(this);
                        if (!isSuccess)
                        {
                            HttpCodeResponseCommand cmd = new HttpCodeResponseCommand();
                            cmd.SetHeader(WebSocketHeader.HttpCode, "1002");
                            sendOverClosed = true;
                            Protocol.WriteCommand(cmd, this);
                            logger.Error("接收数据帧发生异常：{0} {1}", SessionID, EndPoint);
                        }
                        else
                        {
                            //
                            if (FrameReader.IsCompleted)
                            {
                                
                                frameStream.Position = 0;
                                frameStream.SetLength(0);
                                logger.Trace("收到数据帧：{0} {1} {2}", SessionID, EndPoint, FrameReader.ToString());
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
                                logger.Trace("收到持续数据帧：{0} {1} {2}", SessionID, EndPoint, FrameReader.ToString());
                            }
                        }
                    }
                }
                catch
                {
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
