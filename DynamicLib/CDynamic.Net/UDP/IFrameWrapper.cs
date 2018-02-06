using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net
{
    public interface IFrameWrapper
    {
        byte[] Wrapper(byte[] frameData, UdpCommand cmd);
        byte[] UnWrapper(byte[] wrapData);
    }
}
