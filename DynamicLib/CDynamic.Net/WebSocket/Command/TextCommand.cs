using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket.Frames;
using Dynamic.Net.Base;

namespace Dynamic.Net.WebSocket.Command
{
    public class TextCommand : FrameCommandBase
    {
        public string Content { get; set; }

        public TextCommand()
            : base()
        {
        }

        public TextCommand(FrameStreamReader reader)
            : base(reader)
        {
        }

        public override string Name
        {
            get
            {
                return "TextCommand";
            }
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {
            if (InnerData != null && InnerData.Length > 0)
            {
                Content = session.Encoding.GetString(InnerData);
            }

            WebSocketSessionBase ws = session as WebSocketSessionBase;
            if (ws == null)
                return null;

            MessageReceivedEventArgs args = new MessageReceivedEventArgs()
            {
                Content = this.Content,
                ContentType = MessageContentType.Text,
                Data = InnerData,
                Session = ws,
                IsAync = false
            };

            ws.OnMessageReceived(args);

            INetCommand responseCommand = null;
            if (!args.IsAync && !String.IsNullOrEmpty(args.ResponseContent) )
            {
                responseCommand = WebSocketCommandFactory.CreateCommand(args.ResponseContent);
            }

            return responseCommand;
        }


        public override Byte[] GetResponseData(INetSession session)
        {
            return session.Encoding.GetBytes(Content);
        }

        public override string ToString()
        {
            return String.Format("CommandName：{0} \r\nContent：{1}", Name, Content);
        }
    }
}
