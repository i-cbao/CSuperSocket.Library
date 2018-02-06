using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using System.IO;

namespace Dynamic.Net.Protocol
{
    public abstract class ProtocolBase : INetProtocol
    {
        #region INetProtocol 成员

        public abstract IEnumerable<INetCommand> Commands { get; }

        public abstract bool IsFrameEnd(System.IO.Stream stream);

        public abstract INetCommand GetCommand(INetSession session);

        public abstract INetCommand GetCommand(INetSession session,System.IO.Stream stream);

        protected abstract void WriteCommandNameEndBytes(INetCommand command, INetSession session);

        protected abstract void WriteCommandParameterSplitBytes(INetCommand command, INetSession session);

        protected abstract void WriteFrameEndBytes(INetCommand command, INetSession session);

        public virtual void WriteCommand(INetCommand command, INetSession session)
        {
            Byte[] rdata = null;
            if (command != null)
            {
                rdata = command.CommandName;
                if (rdata != null && rdata.Length > 0)
                {
                    session.WriteBytes(rdata, 0, rdata.Length);
                    WriteCommandNameEndBytes(command, session);
                }

                if (command.Parameters != null)
                {
                    foreach (Byte[] pData in command.Parameters)
                    {
                        if (pData != null && pData.Length > 0)
                        {
                            session.WriteBytes(pData, 0, pData.Length);
                        }
                        WriteCommandParameterSplitBytes(command, session);
                    }
                }
            }

            WriteFrameEndBytes(command, session);
        }

        public virtual bool TryGetCommand(INetSession session)
        {
            byte[] tmpData = new byte[1];
            return session.ReadBytes(tmpData, 0, tmpData.Length);
        }

        #endregion
    }
}
