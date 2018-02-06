using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket.Command;
using Dynamic.Net.Base;
using System.IO;
using Dynamic.Net.Common;

namespace Dynamic.Net.WebSocket.Frames
{
    public class FrameStreamWriter
    {
        FrameCommandBase command = null;
        private bool isMask = false;

        public FrameStreamWriter(FrameCommandBase command)
            : this(command, false)
        {
        }

        public FrameStreamWriter(FrameCommandBase command, bool isMask)
        {
            this.command = command;
            this.isMask = isMask;
        }


        public bool Write(INetSession session)
        {
            //第一字节
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            Byte fin = 0;
            fin = (byte)(fin | 0x80);

            Byte opcode = (Byte)command.Opcode;
            opcode = (byte)(opcode & 0x7f);
            fin = (byte)(fin | opcode);

            bw.Write(fin);

            int frameMask = 0;
            if (isMask)
            {
                frameMask = frameMask | 0x80;
            }

            Byte[] responseData = command.GetResponseData(session);
            long length = 0;
            if (responseData != null)
            {
                length = responseData.Length;
            }

            if (length >= 0 && length <= 0x7d)
            {
                frameMask = frameMask | (byte)length;
                bw.Write((byte)frameMask);
            }
            else if (length > 0x7d && length <= 0xffff)
            {
                frameMask = frameMask | 0x7e;
                bw.Write((byte)frameMask);



                bw.Write(BitConverter.GetBytes((UInt16)length).Reverse().ToArray());
            }
            else
            {
                frameMask = frameMask | 0x7f;
                bw.Write((byte)frameMask);
                bw.Write(BitConverter.GetBytes((UInt64)length).Reverse().ToArray());
            }



            if (isMask)
            {
                //四个随机字节
                Random r = new Random();
                Byte[] maskingBytes = new Byte[4];
                r.NextBytes(maskingBytes);
                bw.Write(maskingBytes);

                if (length > 0)
                {
                    responseData.EecodeMask(maskingBytes, 0, responseData.Length);
                }
            }

            if (length > 0)
            {
                bw.Write(responseData);
            }

            stream.Flush();

            stream.Position = 0;
            BinaryReader br = new BinaryReader(stream);

            Byte[] frameData = br.ReadBytes((int)stream.Length);
            session.WriteBytes(frameData, 0, frameData.Length);

            bw.Close();
            return true;
        }

    }
}
