using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Dynamic.Core.Log;
using Dynamic.Net.Base;
using Dynamic.Net.Session;

namespace Dynamic.Net.Server
{
    public class SyncTcpServer : SocketServerBase
    {
        public SyncTcpServer()
        {
          
        }

        private Semaphore maxConntectedSemaphore = null;

    

        public override bool Setup(Dynamic.Net.Base.INetServerConfig config, Dynamic.Net.Base.INetApplication application, Dynamic.Net.Base.INetProtocol protocol)
        {
            if (base.Setup(config, application, protocol, new SyncTcpSessionFactory() ))
            {
                Logger = LoggerManager.GetLogger(String.Concat(Application.Name, ".SyncTcp.", config.Port));
                this.sessionFactory = sessionFactory;
                return true;
            }

            return false;
        }

        protected override bool InnerStart()
        {       
            

            try
            {
                serverSocket = new Socket(config.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                serverSocket.Bind(EndPoint);
                serverSocket.Listen(config.Backlog);

                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            }
            catch (Exception e)
            {
                return false;
            }

            maxConntectedSemaphore = new Semaphore(config.MaxConnectionNumber, config.MaxConnectionNumber);

            Status = Dynamic.Net.Base.NetServerStatus.Started;
            StartupCompleted();

            IsRunning = true;
            while (Status == Dynamic.Net.Base.NetServerStatus.Started)
            {
                Socket client = null;

                try
                {
                    maxConntectedSemaphore.WaitOne();

                    if (Status != Dynamic.Net.Base.NetServerStatus.Started)
                    {
                        break;
                    }

                    client = serverSocket.Accept();
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
                    if (se != null)
                    {
                        if (se.ErrorCode == 10004 || se.ErrorCode == 10038)
                            break;
                    }
                    
                    break;
                }

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

            IsRunning = false;

            return true;
        }

        void session_SessionClosed(object sender, EventArgs e)
        {
            maxConntectedSemaphore.Release();
        }
    }
}
