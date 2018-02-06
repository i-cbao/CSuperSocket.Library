using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class EmptyReplyCommand : ICommand
    {
        private static EmptyReplyCommand empty = new EmptyReplyCommand();

        public static EmptyReplyCommand Empty
        {
            get { return empty; }
        }

        #region ICommand 成员

        public bool CanExecute(WSCommandTypeBase command)
        {
            return false;
        }

        public ICommand Parse(WSCommandTypeBase command)
        {
            return Empty;
        }

        public void ToCommand(WSCommandTypeBase command)
        {
            
        }

        //public WSCommandTypeBase GetEmptyCommand(IWebSocketCommandFactory commandFactory)
        //{
        //    return null;
        //}

        public ICommand Execute(WSCommandTypeBase command, ExecuteCommandContext context)
        {
            return null;
        }
        #endregion

        public override bool Equals(object obj)
        {
            return obj is EmptyReplyCommand;
        }

        public override int GetHashCode()
        {
            return 0;
        }

    }
}
