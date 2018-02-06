using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using NLog;

namespace Dynamic.Net.Pipe
{
    public class PipeClient : PipeBase
    {


        public PipeClient(string readHandler, string writeHandler)
        {
            ClientReadHandler = readHandler;
            ClientWriteHandler = writeHandler;
        }

   



        protected override PipeStream CreateReadStream()
        {
            AnonymousPipeClientStream rs = new AnonymousPipeClientStream(PipeDirection.In, ClientReadHandler);
            rs.ReadMode = PipeTransmissionMode.Byte;

            return rs;
        }

        protected override PipeStream CreateWriteStream()
        {
            AnonymousPipeClientStream ws = new AnonymousPipeClientStream(PipeDirection.Out, ClientWriteHandler);
            ws.ReadMode = PipeTransmissionMode.Byte;

            return ws;

        }
    }
}
