using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class SetCommandParserRequest : CommandBase
    {
        public static Dictionary<String, ICommandParser> Parsers = new Dictionary<String, ICommandParser>()
        {
            { "Binary", new WSBinaryCommandType()},
            { "XML" , new WSCommandType()}
        };


        public SetCommandParserRequest()
            : base("SetCommandParserRequest", "Core", "Type")
        {
        }

        public SetCommandParserRequest(String parserType)
            :this()
        {
            Type = parserType;
        }

        public SetCommandParserRequest(WSCommandTypeBase command)
            :this()
        {
            LoadCommand(command);
        }

        public String Type
        {
            get;
            set;
        }

        protected override void SetCommandParameters(WSCommandTypeBase command)
        {
            
            command.SetCommandParameter("Type", Type);
        }

        public override ICommand Parse(WSCommandTypeBase command)
        {
            SetCommandParserRequest cmd = new SetCommandParserRequest();
            cmd.LoadCommand(command);
            return cmd;
        }

        public override void LoadCommand(WSCommandTypeBase command)
        {

            Type = command.GetCommandParameterStringValue("Type");
        }

        public override ICommand Execute(WSCommandTypeBase command, ExecuteCommandContext context)
        {
            SetCommandParserRequest cmd = new SetCommandParserRequest(command);
            SetCommandParserResponse response = new SetCommandParserResponse();
            response.IsSuccess = false;
            if (context != null && context.CommandSession != null)
            {
                if (Parsers.ContainsKey(cmd.Type))
                {
                    context.CommandSession.CommandParser = Parsers[cmd.Type];
                    response.IsSuccess = true;
                }
                else
                {
                    response.Message = "未知的命令格式";
                }
            }
            else
            {
                response.Message = "内部错误，会话为空";
            }

            return response;
        }
    }
}
