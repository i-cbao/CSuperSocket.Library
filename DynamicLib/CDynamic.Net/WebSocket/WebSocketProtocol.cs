using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Protocol;
using System.IO;
using Dynamic.Net.Common;
using Dynamic.Net.Base;
using Dynamic.Net.WebSocket.Command;
using System.Diagnostics;
using Dynamic.Net.WebSocket.Frames;

namespace Dynamic.Net.WebSocket
{
    public class WebSocketProtocol : ProtocolBase
    {
        private static Byte[] headerEndBytes = Encoding.UTF8.GetBytes("\r\n\r\n");
        private static List<INetCommand> commands = new List<INetCommand>()
        {
            new HandshakeRequestCommand()
        };

        public override IEnumerable<Dynamic.Net.Base.INetCommand> Commands
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsFrameEnd(System.IO.Stream stream)
        {
            try
            {
                if (stream.Length < 4)
                    return false;

                bool isEnd = false;

                stream.Seek(-4, SeekOrigin.End);
                Byte[] tmpData = new byte[4];
                stream.Read(tmpData, 0, 4);
                if (tmpData.AreEquals(headerEndBytes))
                {
                    isEnd = true;
                }

                //for (int i = 0; i < tmpData.Length; i++)
                //{
                //    Debug.WriteLine(tmpData[i]);
                //}

                stream.Position = 0;
                StreamReader sr = new StreamReader(stream);
               // Debug.WriteLine(sr.ReadToEnd());

                stream.Seek(0, SeekOrigin.End);
                return isEnd;
            }
            catch
            {
                return false;
            }
        }

       

        public override Dynamic.Net.Base.INetCommand GetCommand(Dynamic.Net.Base.INetSession session)
        {
            throw new NotImplementedException();
        }

        public override Dynamic.Net.Base.INetCommand GetCommand(INetSession session, System.IO.Stream stream)
        {
            INetCommand command = null;
            StreamReader sr = new StreamReader(stream);
            string header=  sr.ReadToEnd();
            if (header.StartsWith("GET ", StringComparison.OrdinalIgnoreCase))
            {
                HandshakeRequestCommand handshakeCmd = new HandshakeRequestCommand();
                if (handshakeCmd.Parse(header))
                {
                    command = handshakeCmd;
                }
            }
            else
            {
                WebSocketSession wSession = session as WebSocketSession;
                if (wSession != null && wSession.IsHandShake)
                {

                }
                else
                {
                    session.Close();
                }
            }
            return command;
        }

        public override void WriteCommand(INetCommand command, INetSession session)
        {
            if (command is HandshakeResponseCommand)
            {
                HandshakeResponseCommand response = command as HandshakeResponseCommand;
                Byte[] responseData = response.GetResponseData(session);
                session.WriteBytes(responseData, 0, responseData.Length);
            }
            else if (command is FrameCommandBase)
            {
                FrameStreamWriter writer = new FrameStreamWriter(command as FrameCommandBase);
                writer.Write(session);
            }
        }



        protected override void WriteCommandNameEndBytes(Dynamic.Net.Base.INetCommand command, Dynamic.Net.Base.INetSession session)
        {
            return;
        }

        protected override void WriteCommandParameterSplitBytes(Dynamic.Net.Base.INetCommand command, Dynamic.Net.Base.INetSession session)
        {
            return;
        }

        protected override void WriteFrameEndBytes(Dynamic.Net.Base.INetCommand command, Dynamic.Net.Base.INetSession session)
        {
            return;
        }
    }
}
