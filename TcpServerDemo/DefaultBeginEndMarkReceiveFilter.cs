using CSuperSocket.Facility.Protocol;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;


namespace TcpServerDemo
{
    public class DefaultBeginEndMarkReceiveFilter : BeginEndMarkReceiveFilter<RequestInfo>
    {
        private readonly static byte[] BeginMark = new byte[] { (byte)'!' };
        private readonly static byte[] EndMark = new byte[] { (byte)'$' };
        public DefaultBeginEndMarkReceiveFilter()
            : base(BeginMark, EndMark)
        {

        }
        protected override RequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            var key = readBuffer.ToHex((uint)offset, 1);
            return new RequestInfo(key, readBuffer);
        }
    }
}
