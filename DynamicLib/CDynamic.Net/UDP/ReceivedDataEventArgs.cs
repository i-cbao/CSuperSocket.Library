using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Dynamic.Net
{
    public class ReceivedDataEventArgs : EventArgs
    {
        public Byte[] Data { get; private set; }
        public UdpSession Session { get; private set; }
        public bool IsText { get; private set; }

        public ReceivedDataEventArgs(Byte[] data, UdpSession session, bool isText)
        {
            Data = data;
            if (Data == null)
            {
                Data = new byte[0];
            }
            Session = session;
            IsText = isText;
        }
    }
}
