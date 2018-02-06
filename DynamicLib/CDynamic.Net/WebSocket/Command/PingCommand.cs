using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using Dynamic.Net.WebSocket.Frames;

namespace Dynamic.Net.WebSocket.Command
{
    public class PingCommand : FrameCommandBase
    {
        public string Content { get; set; }

        public PingCommand()
        {
        }

        public PingCommand(FrameStreamReader reader)
            : base(reader)
        {
        }

        public override string Name
        {
            get
            {
                return "PingCommand";
            }
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {
           
            WebSocketSession ws = session as WebSocketSession;
            if (ws == null)
                return null;

            PongCommand pongCommand = new PongCommand();
            pongCommand.InnerData = this.InnerData;


            return pongCommand;
        }


    }
}
