using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class SetCommandParserResponse : CommandResponseBase
    {
        public SetCommandParserResponse()
            : base("SetCommandParserResponse", "Core")
        {
        }

        public SetCommandParserResponse(bool isSuccess, String message)
            : this()
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public SetCommandParserResponse(WSCommandTypeBase command)
            : this()
        {
            LoadCommand(command);
        }
        

        public override ICommand Parse(WSCommandTypeBase command)
        {
            SetCommandParserResponse cmd = new SetCommandParserResponse();
            cmd.LoadCommand(command);
            return cmd;
        }

        public override ICommand Execute(WSCommandTypeBase command, ExecuteCommandContext context)
        {
            return null;
        }
    }
}
