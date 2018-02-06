using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket.Frames;

namespace Dynamic.Net.WebSocket.Command
{
    public static class WebSocketCommandFactory
    {
        public static FrameCommandBase CreateCommand(FrameStreamReader reader)
        {
            FrameCommandBase command = null;
            if (reader.Opcode == Opcodes.BinaryFrame)
            {
                command = new BinaryCommand(reader);
            }
            else if (reader.Opcode == Opcodes.ConnectionClose)
            {
                command = new ConnectionCloseCommand(reader);
            }
            else if (reader.Opcode == Opcodes.TextFrame)
            {
                command = new TextCommand(reader);
            }
            else if (reader.Opcode == Opcodes.ContinuationFrame)
            {
            }
            else if (reader.Opcode == Opcodes.Ping)
            {
                command = new PingCommand(reader);
            }
            else if (reader.Opcode == Opcodes.Pong)
            {
                command = new PongCommand(reader);
            }

            return command;
        }


        public static FrameCommandBase CreateCommand(String content)
        {
            TextCommand cmd = new TextCommand()
            {
                Content = content
            };
            cmd.Opcode = Opcodes.TextFrame;

            return cmd;
        }

        public static FrameCommandBase CreateCommand(Byte[] data)
        {
            BinaryCommand cmd = new BinaryCommand()
            {
                InnerData = data,
                Opcode = Opcodes.BinaryFrame
            };

            return cmd;
        }

        public static FrameCommandBase CreateCommand(Byte[] data, int opcodes)
        {
            return new BinaryCommand() { InnerData = data, Opcode = opcodes };
        }
    }
}
