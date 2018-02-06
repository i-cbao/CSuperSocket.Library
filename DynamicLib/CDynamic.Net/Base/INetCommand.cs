using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Base
{
    public interface INetCommand
    {
        Byte[] CommandName { get; }

        IEnumerable<Byte[]> Parameters { get; }

        Encoding Encoding { get; set; }

        bool IsMatch(Byte[] commandName);

        bool SetParameter(Byte[] parValue, int idx);

        INetCommand Execute(INetSession session);
    }
}
