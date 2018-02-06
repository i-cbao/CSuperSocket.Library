using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;

namespace Dynamic.Net.Command
{
    public class PongCommand : CommandBase
    {
        public override string Name
        {
            get { return "pong"; }
        }

        protected override bool ParameterCheck(int idx)
        {
            return idx <1;
        }

        public override INetCommand Execute(INetSession session)
        {
            return null;
        }
    }
}
