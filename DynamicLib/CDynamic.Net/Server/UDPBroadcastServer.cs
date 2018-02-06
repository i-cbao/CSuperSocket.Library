using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Dynamic.Core.Log;
using Dynamic.Net.Base;
using System.Threading;
using Dynamic.Net.Session;

namespace Dynamic.Net.Server
{
    public class UDPBroadcastServer : SocketServerBase
    {
        private ILogger logger = LoggerManager.GetLogger("UDPBroadcastServer");

        public TimeSpan SessionTimeOut { get; set; }

        private UdpClient udpClient = null;

        private List<SessionItem> seesionList = new List<SessionItem>();

        protected override bool InnerStart()
        {
            udpClient = new UdpClient();

            udpClient.ExclusiveAddressUse = false;
            udpClient.EnableBroadcast = true;

            IPEndPoint point = new IPEndPoint(IPAddress.Any, config.Port);

            udpClient.Client.Bind(point);


            //udpState = new UdpState();
            //udpState.Client = udpClient;
            //udpState.EndPoint = e;
            try
            {
                udpClient.JoinMulticastGroup((EndPoint as IPEndPoint).Address);
            }
            catch (Exception ex)
            {
                logger.Error("接收组播失败:{0}", ex.ToString());
                return false;
            }

            IsRunning = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(receive), null);

            return true;
        }

        public override bool Stop()
        {
            Status = NetServerStatus.Stopping;

            return true;
        }

        void receive(object ctx)
        {
            while (true)
            {

                IPEndPoint endPoint = null;
                Byte[] receiveBytes = udpClient.Receive(ref endPoint);
                logger.Trace("收到数据:{0}", receiveBytes.Length);

                SessionItem session = seesionList.FirstOrDefault(x => x.EndPoint == endPoint);
                Dynamic.Net.Session.UdpSession netSession = null;
                if (session == null)
                {
                    netSession = sessionFactory.CreateSession(Application, Protocol, this, udpClient.Client) as Dynamic.Net.Session.UdpSession;
                    if (netSession == null)
                    {
                        throw new NotSupportedException("必须使用UDP会话类型工厂");
                    }

                    seesionList.Add(new SessionItem() { EndPoint = endPoint, Session = netSession });
                    Application.SessionCreated(netSession);
                }
                else
                {
                    netSession = session.Session;
                }

                netSession.ReceivedData(receiveBytes);
                

                if (Status == NetServerStatus.Stopped)
                    break;

            }
        }
    }


    public class SessionItem
    {
        public IPEndPoint EndPoint { get; set; }

        public Dynamic.Net.Session.UdpSession Session { get; set; }

    }
}
