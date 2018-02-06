using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class ConnectedEventArgs : EventArgs
    {
        public CommandClient Client { get; set; }

        public bool IsSuccess { get; set; }
    }
}
