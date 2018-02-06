using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Base
{
    public enum NetServerStatus
    {
        Unknown = 0,
        Uninit,
        Inited,
        Starting,
        Started,
        Stopping,
        Stopped,
        Error
    }
}
