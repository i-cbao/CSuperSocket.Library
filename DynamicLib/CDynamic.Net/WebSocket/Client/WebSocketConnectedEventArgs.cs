using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketConnectedEventArgs : EventArgs
    {
        public bool IsSuccess { get; set; }

        public WebSocketClient Client { get; set; }


        public WebSocketConnectedEventArgs(bool isSuccess, WebSocketClient client)
        {
            IsSuccess = isSuccess;
            Client = client;
        }
    }
}
