using CSuperSocket.Facility.Protocol;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;


namespace TcpServerDemo
{
    public class DefaultBeginEndMarkReceiveFilter2 : BeginEndMarkReceiveFilter<RequestInfo>
    {
        private readonly static byte[] BeginMark = new byte[] { 0x29,0x29 };
        private readonly static byte[] EndMark = new byte[] { 0x0D};
        public DefaultBeginEndMarkReceiveFilter2()
            : base(BeginMark, EndMark)
        {

        }
        public override RequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            var key = readBuffer.ToHex((uint)offset, 1);
            rest = 0;
            //readBuffer
            Console.WriteLine(readBuffer.ToHex());
            return new RequestInfo(key, readBuffer);
            //   return base.Filter(readBuffer, offset, length, toBeCopied, out rest);
        }
        protected override RequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            var key = readBuffer.ToHex((uint)offset, 1);
            return new RequestInfo(key, readBuffer);
        }
    }
}
