using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;

namespace Dynamic.Net.Command
{
    public class PingCommand : CommandBase
    {
        public override string Name
        {
            get { return "ping"; }
        }

        protected override bool ParameterCheck(int idx)
        {
            return false;
        }

        public override INetCommand Execute(INetSession session)
        {
            INetCommand cmd = new PongCommand();
            cmd.Encoding = session.Encoding;
            cmd.SetParameter(
                session.Encoding.GetBytes(
                    String.Format("Response:{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now)),
                    0);

            return cmd;
        }
    }
}
