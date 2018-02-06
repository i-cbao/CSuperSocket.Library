using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket
{
    public enum WebSocketHeader
    {
        Unknown =0,
        Method,
        Url,
        HttpVersion,
        Upgrade,
        Connection,
        Host,
        SecWebSocketOrigin,
        SecWebSocketKey,
        SecWebSocketVersion,
        SecWebSocketAccept,
        SecWebSocketProtocol,
        SwitchingProtocols,
        Cookie,
        HttpCode
    }

    public static class WebSocketHeaderConverter
    {
        public static string ConverterToString(WebSocketHeader header)
        {
            string hString = "";
            switch (header)
            {
                case WebSocketHeader.Connection:
                    hString = "Connection";
                    break;
                case WebSocketHeader.Cookie:
                    hString = "Cookie";
                    break;
                case WebSocketHeader.Host:
                    hString = "Host";
                    break;
                case WebSocketHeader.HttpCode:
                    hString = "HttpCode";
                    break;
                case WebSocketHeader.HttpVersion:
                    hString = "HttpVersion";
                    break;
                case WebSocketHeader.Method:
                    hString = "Method";
                    break;
                case WebSocketHeader.SecWebSocketKey :
                    hString = "Sec-WebSocket-Key";
                    break;
                case WebSocketHeader.SecWebSocketOrigin :
                    hString = "Sec-WebSocket-Origin";
                    break;
                case WebSocketHeader.SecWebSocketVersion:
                    hString = "Sec-WebSocket-Version";
                    break;
                case WebSocketHeader.SecWebSocketAccept:
                    hString = "Sec-WebSocket-Accept";
                    break;
                case WebSocketHeader.SecWebSocketProtocol:
                    hString = "Sec-WebSocket-Protocol";
                    break;
                case WebSocketHeader.Upgrade:
                    hString = "Upgrade";
                    break;
                case WebSocketHeader.Url:
                    hString = "Url";
                    break;
                case WebSocketHeader.SwitchingProtocols:
                    hString = "SwitchingProtocols";
                    break;
            }
            return hString;
        }


        public static WebSocketHeader ConverterToHeader(string hString)
        {
            WebSocketHeader header = WebSocketHeader.Unknown;
            switch (hString)
            {
                case "Connection":
                    header = WebSocketHeader.Connection;
                    break;
                case "Cookie":
                    header = WebSocketHeader.Cookie;
                    break;
                case "Host":
                    header = WebSocketHeader.Host;
                    break;
                case "Sec-WebSocket-Key":
                    header = WebSocketHeader.SecWebSocketKey;
                    break;
                case  "Sec-WebSocket-Origin":
                    header = WebSocketHeader.SecWebSocketOrigin;
                    break;
                case "Sec-WebSocket-Version":
                    header = WebSocketHeader.SecWebSocketVersion;
                    break;
                case "Sec-WebSocket-Accept":
                    header = WebSocketHeader.SecWebSocketAccept;
                    break;
                case "Sec-WebSocket-Protocol":
                    header = WebSocketHeader.SecWebSocketProtocol;
                    break;
                case "Upgrade":
                    header = WebSocketHeader.Upgrade;
                    break;
                case "Url":
                    header = WebSocketHeader.Url;
                    break;
            }
            return header;
        }
    }
}
