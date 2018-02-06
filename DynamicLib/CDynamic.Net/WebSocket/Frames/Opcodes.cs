using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.Frames
{
    public static class Opcodes
    {
        public static readonly int ContinuationFrame = 0x0;
        public static readonly int TextFrame = 0x1;
        public static readonly int BinaryFrame = 0x2;
        public static readonly int ConnectionClose = 0x8;
        public static readonly int Ping = 0x9;
        public static readonly int Pong = 0xa;

        private static List<int> validateCodes = new List<int>()
        {
            ContinuationFrame,
            TextFrame,
            BinaryFrame,
            ConnectionClose,
            Ping,
            Pong
        };

        public static bool IsValidCode(int code)
        {
            return validateCodes.Any(x => x == code);
        }
    }
}
