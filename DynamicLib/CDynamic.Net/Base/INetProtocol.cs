using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dynamic.Net.Base
{
    public interface INetProtocol
    {
        IEnumerable<INetCommand> Commands { get; }

        bool IsFrameEnd(Stream stream);

        bool TryGetCommand(INetSession session);

        INetCommand GetCommand(INetSession session);


        INetCommand GetCommand(INetSession session, Stream stream);

        void WriteCommand(INetCommand command, INetSession session);

    }
}
