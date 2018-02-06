using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class CommandReceivedEventArgs<T> : EventArgs
    {
        public bool IsUnknwon { get; set; }

        public T RequestCommand { get; set; }

        public CommandSession Session { get; set; }

        public T ReceivedCommand { get; set; }

        public T ReplyCommand { get; set; }

        public CommandReceivedEventArgs(T command)
        {
            this.ReceivedCommand = command;
            IsUnknwon = true;
        }
    }
}
