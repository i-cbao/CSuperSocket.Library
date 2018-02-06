using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket.Frames;

namespace Dynamic.Net.WebSocket.Command
{
    public class ConnectionCloseCommand : FrameCommandBase
    {
        public ConnectionCloseCommand()
        {
        }

        public ConnectionCloseCommand(FrameStreamReader reader)
            : base(reader)
        {
        }

        public override string Name
        {
            get
            {
                return "ConnectionClose";
            }
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {
            session.Close();
            return null;
        }

    }
}
