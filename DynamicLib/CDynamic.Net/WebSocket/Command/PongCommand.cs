using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket.Frames;

namespace Dynamic.Net.WebSocket.Command
{
    public class PongCommand : FrameCommandBase
    {
        public PongCommand()
        {
        }

        public PongCommand(FrameStreamReader reader)
            : base(reader)
        {
        }

        public override string Name
        {
            get
            {
                return "PongCommand";
            }
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        { 
            return null;
        }
    }
}
