using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Session
{
    public enum SessionCloseReason
    {
        Unknown = 0,
        Offline,   //服务端中断
        Normal //客户端中断
    }
}
