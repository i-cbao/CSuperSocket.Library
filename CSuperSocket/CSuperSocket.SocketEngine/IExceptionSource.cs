using CSuperSocket.Common;
using System;

namespace CSuperSocket.SocketEngine
{
    interface IExceptionSource
    {
        event EventHandler<ErrorEventArgs> ExceptionThrown;
    }
}
