using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using System.Diagnostics;

namespace Dynamic.Net.WebSocket.Command
{
    public class HandshakeResponseCommand : HandshakeCommandBase
    {
        private static string headerWrap = "\r\n";

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {
            return null;
        }

        public Byte[] GetResponseData(INetSession session)
        {
            StringBuilder sbCommand = new StringBuilder();
            sbCommand.Append(GetHeader(WebSocketHeader.HttpVersion)).Append(" ");
            sbCommand.Append(GetHeader(WebSocketHeader.HttpCode)).Append(" ");
            sbCommand.Append(GetHeader(WebSocketHeader.SwitchingProtocols));
            sbCommand.Append(headerWrap);

            sbCommand.Append(WebSocketHeaderConverter.ConverterToString(WebSocketHeader.Upgrade));
            sbCommand.Append(": ");
            sbCommand.Append(GetHeader(WebSocketHeader.Upgrade));
            sbCommand.Append(headerWrap);

            sbCommand.Append(WebSocketHeaderConverter.ConverterToString(WebSocketHeader.Connection));
            sbCommand.Append(": ");
            sbCommand.Append(GetHeader(WebSocketHeader.Connection));
            sbCommand.Append(headerWrap);

            sbCommand.Append(WebSocketHeaderConverter.ConverterToString(WebSocketHeader.SecWebSocketAccept));
            sbCommand.Append(": ");
            sbCommand.Append(GetHeader(WebSocketHeader.SecWebSocketAccept));
            sbCommand.Append(headerWrap);
            WebSocketSessionBase s = session as WebSocketSessionBase;
            if (s != null && !String.IsNullOrEmpty(s.SubProtocol))
            {
                sbCommand.Append(WebSocketHeaderConverter.ConverterToString(WebSocketHeader.SecWebSocketProtocol));
                sbCommand.Append(": ");
                sbCommand.Append(s.SubProtocol);
                sbCommand.Append(headerWrap);
            }
            sbCommand.Append(headerWrap);

            string response = sbCommand.ToString();
            Debug.WriteLine(response);
            logger.Info("CommandName：{0}\r\nResponseContent：{1}", Name, response);

            return Encoding.GetBytes(response);
        }
    }
}
