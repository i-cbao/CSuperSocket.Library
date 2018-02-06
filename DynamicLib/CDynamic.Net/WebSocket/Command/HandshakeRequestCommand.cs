using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Command;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Dynamic.Net.WebSocket.Command
{
    public class HandshakeRequestCommand : HandshakeCommandBase
    {


        public override string Name
        {
            get { return "handshake"; }
        }

        public bool Parse(string header)
        {
            Regex regex = new Regex("\r\n");
            string[] headers  = regex.Split(header);
            if (headers == null || headers.Length == 0)
                return false;

            string curHead = "";
            for (int i = 0; i < headers.Length; i++)
            {
                curHead = headers[i];
                if (i == 0)
                {
                    string[] rs = curHead.Split(' ');
                    if (rs == null || rs.Length == 0)
                    {
                        return false;
                    }
                    SetHeader(WebSocketHeader.Method, rs[0]);
                    if (rs.Length > 1)
                    {
                        SetHeader(WebSocketHeader.Url, rs[1]);
                    }
                    if (rs.Length > 2)
                    {
                        SetHeader(WebSocketHeader.HttpVersion, rs[2]);
                    }
                    continue;
                }

                string[] hf = curHead.Split(':');
                if (hf.Length == 2)
                {
                    WebSocketHeader h = WebSocketHeaderConverter.ConverterToHeader(hf[0]);
                    if (h != WebSocketHeader.Unknown)
                    {
                        SetHeader(h, hf[1].TrimStart(' '));
                    }
                }
               
            }
            logger.Info("接收到的WebSocket请求：\r\n{0}", header);
            return true;
        }

        protected override bool ParameterCheck(int idx)
        {
            return idx == 0 ;
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {
            string wKey = GetHeader(WebSocketHeader.SecWebSocketKey);
            if (String.IsNullOrEmpty(wKey))
            {
                return null;
            }

            

            wKey = wKey.TrimEnd() + HandshakeCommandBase.WebSocketKeyGuid;

            byte[] data = Encoding.UTF8.GetBytes(wKey);
            byte[] result;

            SHA1 sha = new SHA1CryptoServiceProvider();
            result = sha.ComputeHash(data);

            wKey = Convert.ToBase64String(result);

            HandshakeResponseCommand response = new HandshakeResponseCommand();
            response.SetHeader(WebSocketHeader.HttpVersion, GetHeader(WebSocketHeader.HttpVersion));
            response.SetHeader(WebSocketHeader.HttpCode, "101");
            response.SetHeader(WebSocketHeader.SwitchingProtocols, "Switching Protocols");
            response.SetHeader(WebSocketHeader.Upgrade, "websocket");
            response.SetHeader(WebSocketHeader.Connection, "Upgrade");
            response.SetHeader(WebSocketHeader.SecWebSocketAccept, wKey);
            WebSocketSession s = session as WebSocketSession;
            string ps = GetHeader(WebSocketHeader.SecWebSocketProtocol);
            if (s != null && !String.IsNullOrEmpty(ps))
            {
                SwitchingProtocolEventArgs spargs = new SwitchingProtocolEventArgs(s, GetHeader(WebSocketHeader.SecWebSocketProtocol));
                s.OnSwitchingProtocol(spargs);
                if (!String.IsNullOrEmpty(spargs.SelectedProtocol))
                {
                    response.SetHeader(WebSocketHeader.SecWebSocketProtocol, spargs.SelectedProtocol);
                    s.SubProtocol = spargs.SelectedProtocol;
                }
                else
                {
                    logger.Warn("没有合适的协议！ {0} {1}", session.SessionID, s.EndPoint);
                    return null;
                }
            }


            return response;

        }
    }
}
