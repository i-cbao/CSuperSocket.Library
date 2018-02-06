using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Session
{
    public enum SessionTimeoutType
    {
        Unknown = 0,
        Request,
        Response,
        Active
    }
}
