using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class EventCommandOnExecuteEventArgs<TRequest, TReply> : EventArgs
        where TRequest : ICommand, new()
        where TReply : ICommand, new()
    {
        public TRequest RequestCommand { get; set; }
        public TReply ReplyCommand { get; set; }

        public EventCommandOnExecuteEventArgs(TRequest request, TReply reply)
        {
            RequestCommand = request;
            ReplyCommand = reply;
        }
    }
}
