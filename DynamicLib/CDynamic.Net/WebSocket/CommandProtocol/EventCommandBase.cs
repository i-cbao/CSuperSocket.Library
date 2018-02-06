using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public abstract class EventCommandBase<TRequest, TReply> : CommandBase
        where TRequest : class, ICommand, new()
        where TReply : class, ICommand, new()
    {
        public event EventHandler<EventCommandOnExecuteEventArgs<TRequest, TReply>> ExecuteCommand;


        public EventCommandBase(string name, string type, params string[] parameters)
            : base(name, type, parameters)
        {
        }



        //public override ICommand Execute(WSBinaryCommandTypeBase command, WebSocketSessionBase session)
        //{
            
        //}

        protected virtual void OnExecuteCommand(EventCommandOnExecuteEventArgs<TRequest, TReply> args)
        {
            if (ExecuteCommand != null)
            {
                ExecuteCommand(this, args);
            }
        }

     //   protected abstract void SetCommandParameters(WSCommandTypeBase command);

    //    public abstract ICommand Parse(WSCommandTypeBase command);

        public override ICommand Execute(WSCommandTypeBase command, ExecuteCommandContext context)
        {
            TRequest request = Parse(command) as TRequest;

            TReply reply = null;
            if (typeof(TReply) == typeof(EmptyReplyCommand))
            {
                reply = EmptyReplyCommand.Empty as TReply;
            }

            EventCommandOnExecuteEventArgs<TRequest, TReply> args = new EventCommandOnExecuteEventArgs<TRequest, TReply>(request, reply);
            OnExecuteCommand(args);

            if (args.ReplyCommand != null && !(args.ReplyCommand is EmptyReplyCommand))
            {
                return args.ReplyCommand;
            }

            return null;
        }
    }
}
