using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Dynamic.Net.Common;
using Dynamic.Net.Session;
using Dynamic.Core.Log;

namespace Dynamic.Net.WebSocket.Frames
{
    public enum FrameReaderStep
    {
        None =0,
        FrameFin,
        FrameMask,
        PayloadLength,
        FrameMaskingKey,
        PayoutData,
        ContinuationFrame,
        Completed
    }

    public class FrameStreamReader
    {
        private Stream stream = null;
        private long position = 0;
        private long curPosition = 0;
        private FrameReaderStep ReaderStep = FrameReaderStep.None;

        public int Opcode = 0;

        public int FrameFin = 0;  //0 继续 1最后一个Frame

        public int FrameMask { get; set; }

        public long FramePayloadLength { get; set; }

        public Byte[] FrameMaskingKey { get; set; }

        public Byte[] FrameData { get; set; }

        public int MaxLength { get; set; }

        public DateTime DataReadTime { get; set; }

        private static ILogger logger = LoggerManager.GetLogger("WebsocketFrame");

        public FrameStreamReader(Stream stream)
        {
            this.stream = stream;

            this.position = stream.Position;
            MaxLength = 1024 * 1024 * 20; //29mb
        }

        public void Reset()
        {
            ReaderStep = FrameReaderStep.None;
            Opcode = 0;
            FrameFin = 0;
            FrameMask = 0;
            FramePayloadLength = 0;
            FrameMaskingKey = null;
        }

        public bool IsCompleted
        {
            get
            {
                return ReaderStep == FrameReaderStep.Completed;
            }
        }

        public bool IsContinue
        {
            get
            {
                return ReaderStep == FrameReaderStep.ContinuationFrame;
            }
        }

        public bool ProcessFrame(AsyncTcpSession session)
        {
            this.position = stream.Position;
            if (stream.Length == 1)
            {
                stream.Position = 0;
                Byte fin = (Byte)stream.ReadByte();

                stream.Position = this.position;

                int opcode = (fin & 0xf);
                if (!Opcodes.IsValidCode(opcode))
                {
                    return false;
                }

                Opcode = opcode;
                FrameFin =( (fin & 0x80) >> 7);

                ReaderStep = FrameReaderStep.FrameFin;

                curPosition = 1;
            }
            else if (stream.Length == 2)
            {
                stream.Position = 1;
                int frameMask = stream.ReadByte();
                curPosition = stream.Position;

                FrameMask = ( (frameMask & 0x80)>> 7);

                ReaderStep = FrameReaderStep.FrameMask;

                FramePayloadLength = frameMask & 0x7f;
                if (FramePayloadLength >= 0 && FramePayloadLength <= 0x7d)
                {
                    //7位长
                    ReaderStep = FrameReaderStep.PayloadLength;
                }
                else if (FramePayloadLength == 0x7E)
                {
                    //16位长
                    session.SetNoCheckCount(1);
                }
                else if (FramePayloadLength == 0x7F)
                {
                    //64位长
                    session.SetNoCheckCount(7);
                }
            }
            else if (ReaderStep == FrameReaderStep.FrameMask)
            {
                stream.Position = curPosition;
                BinaryReader br = new BinaryReader(stream);
                
                 if (FramePayloadLength == 0x7E)
                {
                    //16位长
                    Byte[] tmpBytes = br.ReadBytes(2);

                    tmpBytes = tmpBytes.Reverse().ToArray();

                    FramePayloadLength = BitConverter.ToUInt16(tmpBytes, 0);
                }
                else if (FramePayloadLength == 0x7F)
                {
                    //64位长
                    Byte[] tmpBytes = br.ReadBytes(8);

                    tmpBytes = tmpBytes.Reverse().ToArray();

                    FramePayloadLength =(long) BitConverter.ToUInt64(tmpBytes, 0);
                }
                 curPosition = stream.Position;
                 if (FramePayloadLength > MaxLength)
                 {
                     session.Close();
                     return false;
                 }

                 ReaderStep = FrameReaderStep.PayloadLength;
            }

            if (ReaderStep == FrameReaderStep.PayloadLength)
            {
                if (FrameMask == 1)
                {
                    session.SetNoCheckCount(3);
                    ReaderStep = FrameReaderStep.FrameMaskingKey;
                    return true;
                }
                else
                {
                    if (FramePayloadLength > 0)
                    {
                        session.SetNoCheckCount(FramePayloadLength - 1);
                        DataReadTime = DateTime.Now;
                        ReaderStep = FrameReaderStep.PayoutData;
                    }
                    return true;
                }
            }
            else if (ReaderStep == FrameReaderStep.FrameMaskingKey)
            {
                stream.Position = curPosition;
                BinaryReader br = new BinaryReader(stream);
                FrameMaskingKey = br.ReadBytes(4);
                curPosition = stream.Position;

                if (FramePayloadLength > 0)
                {
                    session.SetNoCheckCount(FramePayloadLength - 1);
                    ReaderStep = FrameReaderStep.PayoutData;
                    return true;
                }
                else
                {
                    ReaderStep = FrameReaderStep.PayoutData;
                }
            }

            //if (ReaderStep == FrameReaderStep.FrameMaskingKey)
            //{
            //    if (FramePayloadLength > 0)
            //    {
            //        session.SetNoCheckCount(FramePayloadLength - 1);
            //        ReaderStep = FrameReaderStep.PayoutData;
            //        return true;
            //    }
            //    else
            //    {
            //        ReaderStep = FrameReaderStep.PayoutData;
            //    }
            //}

            if (ReaderStep == FrameReaderStep.PayoutData)
            {
                if (FramePayloadLength > 0)
                {
                    stream.Position = curPosition;
                    BinaryReader br = new BinaryReader(stream);
                    if (FrameData != null && FrameData.Length > 0)
                    {
                        Byte[] tmpData = FrameData;
                        logger.Trace("新建：{0}", tmpData.Length + FramePayloadLength);
                        FrameData = new Byte[tmpData.Length + FramePayloadLength];
                        Array.Copy(tmpData, FrameData, tmpData.Length);
                        Byte[] curData = br.ReadBytes((int)FramePayloadLength);
                        if (FrameMask == 1)
                        {
                            curData.DecodeMask(FrameMaskingKey, 0, curData.Length);
                        }
                        Array.Copy(curData, 0, FrameData, tmpData.Length, curData.Length);
                    }
                    else
                    {

                        FrameData = br.ReadBytes((int)FramePayloadLength);
                        if (FrameMask == 1)
                        {
                            FrameData.DecodeMask(FrameMaskingKey, 0, FrameData.Length);
                        }
                    }
                }

               

                if (FrameFin == 0)
                {
                    ReaderStep = FrameReaderStep.ContinuationFrame;
                }
                else
                {

                    DataReadTime = DateTime.MinValue;
                    ReaderStep = FrameReaderStep.Completed;
                }
            }

            stream.Position = this.position;
            return true;
        }

        public override string ToString()
        {
            return String.Format("FrameFin：{0}， Opcode：{1}， FrameMask：{2}， PayloadLength：{3}",
                FrameFin, Opcode, FrameMask, FramePayloadLength);
        }
    }
}
