using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Dynamic.Net.KcpSharp.NetWork
{

    public delegate void ReceiveDataEventHandler(
        object sender,
        ReceiveDataEventArgs e);

    public class ReceiveDataEventArgs : EventArgs
    {
        private byte[] _buffer;
        private IPEndPoint _remoteIP;

        public ReceiveDataEventArgs() { }

        public ReceiveDataEventArgs(byte[] buffer, IPEndPoint remoteIP)
            : base()
        {
            _buffer = buffer;
            _remoteIP = remoteIP;
        }

        public byte[] Buffer
        {
            get { return _buffer; }
            set { _buffer = value; }
        }

        public IPEndPoint RemoteIP
        {
            get { return _remoteIP; }
            set { _remoteIP = value; }
        }
    }
}
