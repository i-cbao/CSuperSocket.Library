using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Command;
using Dynamic.Net.WebSocket.Frames;
using Dynamic.Net.Base;
using Dynamic.Core.Log;

namespace Dynamic.Net.WebSocket.Command
{
    public class FrameCommandBase : CommandBase
    {
        protected ILogger logger = null;

        public int Opcode { get; set; }

        public Byte[] InnerData { get; set; }

        public FrameCommandBase()
        {
            logger = LoggerManager.GetLogger("WebSocketCommand");
        }

        public FrameCommandBase(FrameStreamReader reader)
            : this()
        {
            Opcode = reader.Opcode;
            InnerData = reader.FrameData;
        }

        public override string Name
        {
            get { return ""; }
        }

        protected override bool ParameterCheck(int idx)
        {
            return false;
        }

        public override Dynamic.Net.Base.INetCommand Execute(Dynamic.Net.Base.INetSession session)
        {
            return null;
        }


        public virtual Byte[] GetResponseData(INetSession session)
        {
            return InnerData;
        }

        public override string ToString()
        {
            return String.Format("CommandName：{0}\r\nCommandLength：{1}", Name, InnerData == null ? 0 : InnerData.Length);
        }
    }
}
