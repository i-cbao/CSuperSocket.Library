using System;

namespace Dynamic.Net.WebSocket
{
    public class SessionIDChangedEventArgs : EventArgs
    {
        public String Old { get; set; }

        public String New { get; set; }

        public SessionIDChangedEventArgs(string old, string n)
        {
            Old = old;
            New = n;
        }
    }
}
