using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Dynamic.Net
{
    public interface IUdpSessionFactory
    {
        UdpSession CreateSession(UdpSocket client, DnsEndPoint target);
    }
}
