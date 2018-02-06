using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Pipe
{
    public class PipeServer : PipeBase
    {
        


        public override void Stop()
        {
            AnonymousPipeServerStream rs = readStream as AnonymousPipeServerStream;
            AnonymousPipeServerStream ws = writeStream as AnonymousPipeServerStream;
            if (rs != null)
            {
                rs.DisposeLocalCopyOfClientHandle();
            }
            if (ws != null)
            {
                ws.DisposeLocalCopyOfClientHandle();
            }

            base.Stop();
        }

        protected override PipeStream CreateReadStream()
        {
            AnonymousPipeServerStream rs = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            rs.ReadMode = PipeTransmissionMode.Byte;
            ClientWriteHandler = rs.GetClientHandleAsString();
            return rs;
        }

        protected override PipeStream CreateWriteStream()
        {
            AnonymousPipeServerStream ws = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            ws.ReadMode = PipeTransmissionMode.Byte;
            ClientReadHandler = ws.GetClientHandleAsString();

            return ws;
        }
    }
}
