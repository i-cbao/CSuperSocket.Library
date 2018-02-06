using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.Base;
using Dynamic.Net.Command;
using System.IO;

namespace Dynamic.Net.Protocol
{
    /// <summary>
    /// 这是一个测试协议。每条命令以两个0x00结尾
    /// 命令名称后面跟0xFF。参数之间已0x00分隔
    /// </summary>
    public class NormalProtocol : ProtocolBase
    {


        private Stream readStream = null;

        private static List<INetCommand> commands = new List<INetCommand>()
        {
            new PingCommand(),
            new PongCommand()
        };

        private static byte[] splitCommandBytes = new byte[2] { 0, 0 };

        #region INetProtocol 成员

        public override IEnumerable<INetCommand> Commands
        {
            get { return commands; }
        }

        #endregion

        public override INetCommand GetCommand(INetSession session)
        {
            if (readStream == null)
            {
                readStream = new MemoryStream();
            }
            else
            {
                readStream.SetLength(0);
                readStream.Position = 0;
            }
            Byte[] data = new Byte[2];


            while (session.ReadBytes(data, 0, 1))
            {
                if (data[0] == 0x00)
                {
                    session.ReadBytes(data, 1, 1);
                    if (data[1] == 0x00)
                    {
                        // 获取到整条通讯
                        break;
                    }
                    else
                    {
                        readStream.Write(data, 0, 2);
                    }
                }
                else
                {
                    readStream.Write(data, 0, 1);
                }
            }

            readStream.Position = 0;

            return GetCommand(session,readStream);
        }

        public override INetCommand GetCommand(INetSession session,Stream stream)
        {
            INetCommand command = null;
            List<Byte> sb = new List<Byte>();
            int c = 0;
            int parIdx = 0;
            while (true)
            {
                c = stream.ReadByte();
                if (c == -1)
                {
                    break;
                }
                if (c == 0xff && command == null)
                {
                    Byte[] cmdNam = sb.ToArray();
                    command = Commands.FirstOrDefault(x => x.IsMatch(cmdNam));
                    sb.Clear();

                    if (command == null)
                    {
                        break;
                    }
                }
                else if (command != null && c == 0xff)
                {
                    if (!command.SetParameter(sb.ToArray(), parIdx))
                    {
                        command = null;
                        break;
                    }

                    parIdx++;
                }
                else
                {
                    sb.Add((byte)c);
                }
            }

            return command;
        }


        protected override void WriteCommandNameEndBytes(INetCommand command, INetSession session)
        {
            session.WriteBytes(new byte[] { 0xff }, 0, 1);
        }

        protected override void WriteCommandParameterSplitBytes(INetCommand command, INetSession session)
        {
            session.WriteBytes(new byte[] { 0xff }, 0, 1);
        }

        protected override void WriteFrameEndBytes(INetCommand command, INetSession session)
        {
            session.WriteBytes(splitCommandBytes, 0, splitCommandBytes.Length);
        }

        public override bool IsFrameEnd(Stream stream)
        {
            if (stream.Length < 2)
                return false;
            stream.Seek(-2, SeekOrigin.End);
            if (stream.ReadByte() == 0 && stream.ReadByte() == 0)
            {
                stream.Seek(0, SeekOrigin.End);
                return true;
            }
            stream.Seek(0, SeekOrigin.End);
            return false;
        }
    }
}
