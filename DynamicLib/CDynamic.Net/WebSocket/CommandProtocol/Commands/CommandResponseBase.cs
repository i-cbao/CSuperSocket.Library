using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public abstract class CommandResponseBase : CommandBase
    {
        public CommandResponseBase(string name, string type, params string[] parameters)
            : base(name, type, "IsSuccess", "Message")
        {
            if (parameters != null && parameters.Any())
            {
                commandParameters.AddRange(parameters);
            }
        }

        public Boolean IsSuccess { get; set; }

        public String Message { get; set; }

        protected override void SetCommandParameters(WSCommandTypeBase command)
        {

            command.SetCommandParameter("IsSuccess", IsSuccess);
            command.SetCommandParameter("Message", Message);
        }

        public override void LoadCommand(WSCommandTypeBase command)
        {
            IsSuccess = command.GetCommandParameterBooleanValue("IsSuccess");
            Message = command.GetCommandParameterStringValue("Message");

                
        }
    }
}
