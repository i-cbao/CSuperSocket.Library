using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket
{
    public class SwitchingProtocolEventArgs : EventArgs
    {
        public WebSocketSession Session { get; set; }

        public List<String> Protocols { get; set; }

        public string SelectedProtocol { get; set; }

        public SwitchingProtocolEventArgs(WebSocketSession session, string protocol)
        {
            Session = session;
            Protocols = new List<string>();
            if (!String.IsNullOrEmpty(protocol))
            {
                string[] ps = protocol.Split(',');
                ps.All(x =>
                {
                    if (x != null)
                    {
                        Protocols.Add(x.TrimStart().TrimEnd());
                    }
                    return true;
                });
            }
        }
    }
}
