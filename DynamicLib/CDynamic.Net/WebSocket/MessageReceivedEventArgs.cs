using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket
{
    public enum MessageContentType
    {
        Unknown = 0,
        Text,
        Binary
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public WebSocketSessionBase Session { get; set; }

        public MessageContentType ContentType { get; set; }

        public bool IsAync { get; set; }

        public Byte[] Data { get; set; }

        public String Content { get; set; }

        public String ResponseContent { get; set; }

        public Byte[] ResponseData { get; set; }

        public override string ToString()
        {
            return (ContentType == MessageContentType.Text ? Content : (Data == null ? "0" : Data.Length.ToString()));

        }
    }
}
