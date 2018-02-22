using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServerDemo
{
   public  class Service : AppServer<Session, RequestInfo>
    {
        public Service()
          : base(new DefaultReceiveFilterFactory<DefaultBeginEndMarkReceiveFilter, RequestInfo>())
        {
        
        }
        protected override void OnSessionClosed(Session session, CloseReason reason)
        {
            Console.WriteLine("session连接断开" + session.RemoteEndPoint);
            base.OnSessionClosed(session, reason);
            session.Close(CloseReason.TimeOut);
        }
        protected override void OnNewSessionConnected(Session session)
        {
            Console.WriteLine("新的session连接上来" + session.RemoteEndPoint);
            base.OnNewSessionConnected(session);
        }
        protected override void ExecuteCommand(Session session, RequestInfo requestInfo)
        {
          var strValue=System.Text.Encoding.UTF8.GetString(requestInfo.Body);
            Console.WriteLine(strValue);
           
        }
    }
}
