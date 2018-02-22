using CSuperSocket.Facility.Protocol;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServerDemo
{
    public class DefaultFixedHeaderReceiveFilter : FixedHeaderReceiveFilter<RequestInfo>
    {
        public DefaultFixedHeaderReceiveFilter():base(6)
        {

        }
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            return (int)header[offset + 4] * 256 + (int)header[offset + 5];
        }

        protected override RequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            var buffer = new byte[length];
            Array.Copy(bodyBuffer, offset, buffer, 0, buffer.Length);

            return new RequestInfo(Encoding.UTF8.GetString(header.Array, header.Offset, 4), buffer);
            
        }
    }
}
