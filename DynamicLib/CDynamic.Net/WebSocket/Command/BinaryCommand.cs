using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using Dynamic.Net.WebSocket.Frames;

namespace Dynamic.Net.WebSocket.Command
{
    public class BinaryCommand : FrameCommandBase
    {
        public string Content { get; set; }

        public BinaryCommand()
            : base()
        {
        }

        public BinaryCommand(FrameStreamReader reader)
            : base(reader)
        {
        }

        public override string Name
        {
            get
            {
                return "BinaryCommand";
            }
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {

            WebSocketSessionBase ws = session as WebSocketSessionBase;
            if (ws == null)
                return null;

            MessageReceivedEventArgs args = new MessageReceivedEventArgs()
            {
                ContentType = MessageContentType.Binary,
                Data = InnerData,
                Session = session as WebSocketSessionBase,
                IsAync = false
            };

            ws.OnMessageReceived(args);

            INetCommand responseCommand = null;
            if (!args.IsAync && args.ResponseData != null && args.ResponseData.Length > 0 )
            {
                responseCommand = WebSocketCommandFactory.CreateCommand(args.ResponseData);
            }

            return responseCommand;
        }

       
    }
}
