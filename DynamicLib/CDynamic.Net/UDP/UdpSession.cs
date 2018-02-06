using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;


namespace Dynamic.Net
{




    public class UdpSession
    {
        public DateTime StartTime { get; set; }

        public DateTime LastSendTime { get; set; }

        public DateTime LastReceiveTime { get; set; }

        public DateTime ActiveTime { get; set; }

        public double ReceivedBytes { get; set; }

        public double SendedBytes { get; set; }

        public double ErrorBytes { get; set; }

        public DnsEndPoint Target { get; set; }

        public UdpSocket Client { get; set; }

        public UInt16 ProxyID { get; set; }

        public IntPtr ProxyIntPtr { get; set; }

        public String SessionID { get; set; }

        public String Url { get; set; }

        public IntPtr ConnectID { get; set; }

        public event EventHandler Closed;

        public event EventHandler<ReceivedDataEventArgs> ReceivedData;

        public UdpSession()
        {
            SessionID = Guid.NewGuid().ToString("N");
        }


        public virtual void Close()
        {
            if (Client is UdpSocketClient)
            {
                (Client as UdpSocketClient).Close(ProxyID);
            }
            else
            {
                Client.Close();
            }
        }

        public virtual void Send(byte[] data)
        {
            if (Client != null)
            {
                (Client as UdpSocketClient).Send(data, ProxyID);
            }
        }

        //public virtual bool SendSync(byte[] data)
        //{
        //    if (Client != null)
        //    {
        //       return (Client as UdpSocketClient).SendSync(data, ProxyID);
        //    }
        //    return false;
        //}


        public virtual void SendText(string text)
        {
            if (Client != null)
            {
                (Client as UdpSocketClient).SendText(text, ProxyID);
            }
        }

        //public virtual bool SendTextSync(string text)
        //{
        //    if (Client != null)
        //    {
        //        return (Client as UdpSocketClient).SendTextSync(text, ProxyID);
        //    }
        //    return false;
        //}

        internal virtual void OnClosed()
        {
            if (Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
        }


        internal virtual void RaiseReceivedData(byte[] data,bool isText)
        {
            OnReceivedData(data, isText);
        }

        protected virtual void OnReceivedData(byte[] data, bool isText)
        {

            EventHandler<ReceivedDataEventArgs> h = ReceivedData;
            if (h != null)
            {
                h(this, new ReceivedDataEventArgs(data, this, isText));
            }
        }

    }

  

}
