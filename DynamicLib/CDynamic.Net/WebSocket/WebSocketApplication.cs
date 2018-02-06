using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Application;
using Dynamic.Net.Base;
using Dynamic.Net.WebSocket.Command;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketApplication : NetApplicationBase
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public event EventHandler<SwitchingProtocolEventArgs> SwitchingProtocol;

        public event EventHandler HandshakeCompleted;

        public override void SessionCreated(Dynamic.Net.Base.INetSession session)
        {
            base.SessionCreated(session);
            WebSocketSessionBase ws = session as WebSocketSessionBase;
            if (ws != null)
            {
                ws.MessageReceived += new EventHandler<MessageReceivedEventArgs>(ws_MessageReceived);
                ws.SwitchingProtocol += new EventHandler<SwitchingProtocolEventArgs>(ws_SwitchingProtocol);
                ws.HandshakeCompleted += new EventHandler(ws_HandshakeCompleted);
            }
        }

        public virtual void AttachSession(WebSocketSessionBase session)
        {
            if (session != null)
            {
                base.SetSession(session);
                SessionCreated(session);
            }
        }

        public virtual void DettachSession(WebSocketSessionBase session)
        {
            if (session != null)
            {
                session.MessageReceived -= ws_MessageReceived;
                session.SwitchingProtocol -= ws_SwitchingProtocol;
                session.HandshakeCompleted -= ws_HandshakeCompleted;
                RemoveSession(session);
            }
        }


       

        public void Broadcast(string content)
        {
            Broadcast(content, (s) => { return true; });
        }

        public void Broadcast(string content, Func<WebSocketSession, bool> check)
        {
            Func<INetSession, bool> innerCheck = (session) =>
            {
                if (session is WebSocketSession)
                {
                    return check(session as WebSocketSession);
                }
                return false;
            };
            FrameCommandBase cmd = WebSocketCommandFactory.CreateCommand(content);
            if (cmd != null)
            {
                Broadcast(cmd, innerCheck);
                Logger.Trace("广播消息：\r\n{0}", cmd.ToString());
            }
        }

        public void Broadcast(Byte[] data)
        {
            Broadcast(data, (s) => { return true; });
        }

        public void Broadcast(Byte[] data, Func<WebSocketSession, bool> check)
        {
            Func<INetSession, bool> innerCheck = (session) =>
            {
                if (session is WebSocketSession)
                {
                    return check(session as WebSocketSession);
                }
                return false;
            };
            FrameCommandBase cmd = WebSocketCommandFactory.CreateCommand(data);
            if (cmd != null)
            {
                Broadcast(cmd, innerCheck);
                Logger.Trace("广播消息：\r\n{0}", cmd.ToString());
            }
        }

        void ws_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }
        }

        void ws_SwitchingProtocol(object sender, SwitchingProtocolEventArgs e)
        {
            if (SwitchingProtocol != null)
            {
                SwitchingProtocol(sender, e);
            }
        }

        void ws_HandshakeCompleted(object sender, EventArgs e)
        {
            if (HandshakeCompleted != null)
            {
                HandshakeCompleted(sender, e);
            }
        }
    }
}
