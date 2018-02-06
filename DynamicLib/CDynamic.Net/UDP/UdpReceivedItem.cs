using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Dynamic.Net
{
    public class UdpReceivedItem
    {
        public IPEndPoint Source { get; set; }

        public int PackageSeq { get; set; }

        public UInt16 ProxyID { get; set; }

        public UdpPackage Package { get; set; }


        public DateTime LastTime { get; set; }
    }
}
