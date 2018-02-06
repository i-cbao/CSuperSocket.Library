using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace Dynamic.Net
{
    class SendItem
    {
        public UdpFrame Frame { get; set; }

        public DateTime Time { get; set; }

        public DateTime LastSendTime { get; set; }

        public EndPoint Target { get; set; }

        public bool Timeout { get; set; }

        public DateTime TimeoutTime { get; set; }
        
        public int RetryCount { get; set; }
    }
}
