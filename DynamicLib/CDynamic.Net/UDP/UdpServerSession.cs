using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net
{
    public class UdpServerSession : UdpSession
    {
        public new UdpServerSocketProxy Client
        {
            get
            {
                return base.Client as UdpServerSocketProxy;
            }
            set
            {
                base.Client = value;
            }
        }

        public override void Send(byte[] data)
        {
            Client.Send(data, ProxyID);
        }

        //public override bool SendSync(byte[] data)
        //{
        //    return Client.SendSync(data, ProxyID);
        //}


        public override void SendText(string text)
        {
            Client.SendText(text, ProxyID);
        }

        //public override bool SendTextSync(string text)
        //{
        //    return Client.SendTextSync(text, ProxyID);
        //}

        public override void Close()
        {
            Client.Server.CloseClient(Target, ProxyID);
        }

    }
}
