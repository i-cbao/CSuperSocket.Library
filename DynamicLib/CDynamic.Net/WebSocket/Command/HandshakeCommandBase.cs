using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Command;
using Dynamic.Core.Log;

namespace Dynamic.Net.WebSocket.Command
{
    public abstract class HandshakeCommandBase : CommandBase
    {
        private ILogger _logger;
        protected ILogger logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LoggerManager.GetLogger("WebSocketCommand");
                }
                return _logger;
            }
        }

        public static string WebSocketKeyGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private Dictionary<WebSocketHeader, string> headers = new Dictionary<WebSocketHeader, string>();

        public Dictionary<WebSocketHeader, string> Headers
        {
            get
            {
                return headers;
            }
        }

        public virtual string GetHeader(WebSocketHeader header)
        {
            if (headers.ContainsKey(header))
            {
                return headers[header];
            }
            return null;
        }

        public virtual void SetHeader(WebSocketHeader header, string value)
        {
            if (headers.ContainsKey(header))
            {
                headers[header] = value;
            }
            else
            {
                headers.Add(header, value);
            }
        }

        public virtual void RemoveHeader(WebSocketHeader header)
        {
            if (headers.ContainsKey(header))
            {
                headers.Remove(header);
            }
        }

        public override string Name
        {
            get
            {
                return "";
            }
        }

        protected override bool ParameterCheck(int idx)
        {
            return idx == 0;
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {
            return null;
        }
    }
}
