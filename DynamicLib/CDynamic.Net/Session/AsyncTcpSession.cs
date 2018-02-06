using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace Dynamic.Net.Session
{
    public class AsyncTcpSession : SocketSession
    {
        SocketAsyncEventArgs asyncEventArgs = null;
        protected Stream frameStream = new MemoryStream();
        protected bool isNoCheck = false;
        protected long noCheckCount = 0;

        public AsyncTcpSession(INetApplication application, INetProtocol protocol, INetServer server, Socket clientSocket)
            : base(application, protocol, server, clientSocket)
        {
            asyncEventArgs = new SocketAsyncEventArgs();
            Byte[] buffer = new byte[255];
            asyncEventArgs.SetBuffer(buffer, 0, 255);
            asyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AsyncEventArgs_Completed);
        }

       protected virtual  void AsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred <= 0)
            {
                Offline();
                return;
            }



            if (e.SocketError != SocketError.Success)
            {
                Offline();
                return;
            }

            ReceivedBytes += e.BytesTransferred - e.Offset; 

            for (int i = e.Offset; i < e.Offset + e.BytesTransferred; i++)
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

                if (Protocol.IsFrameEnd(frameStream))
                {
                    frameStream.Position = 0;
                    INetCommand command = Protocol.GetCommand(this,frameStream);
                    if (command != null)
                    {
                        frameStream.Position = 0;
                        frameStream.SetLength(0);
                        command = command.Execute(this);
                    }
                    Protocol.WriteCommand(command, this);
                }
                
            }
            
            

            if (!isClosed)
            {
                Protocol.TryGetCommand(this);
            }
           
        }

        public override void Start()
        {
            this.isClosed = false;
            CloseReason = SessionCloseReason.Normal;
            OnSessionStarted();

            Protocol.TryGetCommand(this);
          
        }

        public override bool Close()
        {
            frameStream.Dispose();
            return base.Close();
        }

        public override bool ReadBytes(byte[] data, int start, int count)
        {
          //  isNoCheck = false;
          //  asyncEventArgs.SetBuffer(data, start, count);
            try
            {
                LastRequestTime = DateTime.Now;
                ActiveTime = DateTime.Now;
                bool isAsync = clientSocket.ReceiveAsync(asyncEventArgs);
                if (!isAsync)
                {
                    AsyncEventArgs_Completed(clientSocket, asyncEventArgs);
                }
            }
            catch
            {
                Offline();
            }
            return true;
        }

        public virtual void SetNoCheckCount(long count)
        {
            isNoCheck = true;
            noCheckCount = count;
        }

        public override bool WriteBytes(byte[] data, int start, int count)
        {
            try
            {
                LastResponseTime = DateTime.Now;
                ActiveTime = DateTime.Now;
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(args_Completed);
                args.SetBuffer(data, start, count);

                var c = clientSocket;
                if (!isClosed && c != null && c.Connected)
                {
                    c.SendAsync(args);
                }
            }
            catch
            {
                
                    Offline();
                
            }
            return true;
        }


        void args_Completed(object sender, SocketAsyncEventArgs e)
        {
            SendedBytes += e.Buffer.Length;
            e.SetBuffer(null, 0, 0);
            if (e.SocketError != SocketError.Success)
            {
                
                    Offline();
                
            }
        }
    }
}
