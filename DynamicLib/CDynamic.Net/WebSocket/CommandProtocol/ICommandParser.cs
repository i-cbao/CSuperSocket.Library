using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public interface ICommandParser
    {
        string ParserID { get;}

        MessageContentType TransferEncoder { get; }

        WSCommandTypeBase Create();

        bool CanRead(MessageReceivedEventArgs args, out WSCommandTypeBase command);

        void SetReplyCommand(MessageReceivedEventArgs args, WSCommandTypeBase command);

        byte[] ToBinary(WSCommandTypeBase command);
    }
}
