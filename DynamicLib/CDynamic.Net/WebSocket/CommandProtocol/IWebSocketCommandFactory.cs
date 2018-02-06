using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public interface IWebSocketCommandFactory
    {
        MessageContentType ContentType { get; }

        WSCommandTypeBase CreateCommand();

        IWebSocketCommandFactory GetCommandFactory(MessageReceivedEventArgs args);

        WSCommandTypeBase GetCommand(MessageReceivedEventArgs args);

        void SetReplyCommandData(WSCommandTypeBase command, MessageReceivedEventArgs args);
    }
}
