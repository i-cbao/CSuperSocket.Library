using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Core.Log;
using System.Net.Sockets;
using System.Threading;
using Dynamic.Net.Base;
using Dynamic.Net.Session;

namespace Dynamic.Net.Server
{
    public class AsyncTcpServer : SocketServerBase
    {
       private Semaphore maxConntectedSemaphore = null;

        public override bool Setup(INetServerConfig config, INetApplication application, INetProtocol protocol, ISocketSessionFactory sessionFactory)
        {
            if (base.Setup(config, application, protocol, sessionFactory))
            {
                Logger = LoggerManager.GetLogger(String.Concat(Application.Name, ".AsyncTcp.", config.Port));
                this.sessionFactory = sessionFactory;
                return true;
            }

            return false;
        }

        public override bool Setup(Dynamic.Net.Base.INetServerConfig config, Dynamic.Net.Base.INetApplication application, Dynamic.Net.Base.INetProtocol protocol)
        {
            if (base.Setup(config, application, protocol, new AsyncTcpSessionFactory()))
            {
                Logger = LoggerManager.GetLogger(String.Concat(Application.Name, ".AsyncTcp.", config.Port));
               // this.sessionFactory = sessionFactory;
                return true;
            }

            return false;
        }

        protected override bool InnerStart()
        {
            try
            {
                serverSocket = new Socket(config.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

               // serverSocket.SetSocketOption(SocketOptionLevel.IP, (SocketOptionName)27, 0);
                serverSocket.Bind(EndPoint);
                serverSocket.Listen(config.Backlog);

                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                Logger.Info("监听已启动：{0}", EndPoint.ToString());
            }
            catch (Exception e)
            {
                Logger.Error("启动监听时发生异常：{0}", e.ToString());
                return false;
            }

            maxConntectedSemaphore = new Semaphore(config.MaxConnectionNumber, config.MaxConnectionNumber);

            Status = Dynamic.Net.Base.NetServerStatus.Started;
            StartupCompleted();

            IsRunning = true;
            while (Status == Dynamic.Net.Base.NetServerStatus.Started)
            {
                try
                {
                    maxConntectedSemaphore.WaitOne();

                    if (Status != Dynamic.Net.Base.NetServerStatus.Started)
                    {
                        maxConntectedSemaphore.Release();
                        break;
                    }

                    SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArg_Completed);

                    if (!serverSocket.AcceptAsync(acceptEventArg))
                    {
                        aceptClient(acceptEventArg);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (Exception e)
                {
                    SocketException se = e as SocketException;
                    Logger.Error("客户端连接异常：{0}", e.ToString());
                    maxConntectedSemaphore.Release();
                    break;
                }
               
            }

            IsRunning = false;

            return true;
        }

        private void aceptClient(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket client = e.AcceptSocket;
                if (client != null)
                {
                    INetSession session = sessionFactory.CreateSession(this.Application, this.Protocol, this, client);
                    this.Application.SessionCreated(session);
                    session.SessionClosed += new EventHandler(session_SessionClosed);
                    session.Start();
                }
                else
                {
                    maxConntectedSemaphore.Release();
                }
            }
            else
            {
                maxConntectedSemaphore.Release();
            }
        }

        void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            aceptClient(e);
        }

        void session_SessionClosed(object sender, EventArgs e)
        {
            maxConntectedSemaphore.Release();
        }
    }
}
