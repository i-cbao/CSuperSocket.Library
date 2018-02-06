using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Base
{
    public interface INetServer
    {
        event EventHandler Started;

        INetProtocol Protocol { get; }

        INetApplication Application { get; }

        bool Setup(INetServerConfig config, INetApplication application, INetProtocol protocol);

        bool Start();

        bool Stop();
    }
}
