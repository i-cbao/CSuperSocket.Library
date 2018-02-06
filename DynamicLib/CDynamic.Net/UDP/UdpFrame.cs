using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net
{



     public class UdpFrame
    {
        public UdpCommand Command { get; set; }

        public int PackageSeq { get; set; }
        public UInt16 Seq { get; set; }
        public UInt16 Length { get; set; }
        //代理转发编号，高一位0非代理，1代理
        public UInt16 ProxyID { get; set; }

        public IntPtr ProxyIntPtr { get; set; }


        public byte[] Data { get; set; }

        public bool IsSended { get;  set; }

        public bool IsRemoved { get; set; }

        public bool IsProxy { get; set; }

        public byte[] UDPData { get; private set; }

        public DateTime LastSendTime { get; set; }
       

        public static readonly int HeadLength = 11;


        public UdpFrame(int pSeq, UInt16 seq, UInt16 len, UdpCommand cmd, byte[] data)
        {
            PackageSeq = pSeq;
            Seq = seq;
            Command = cmd;
            Data = data;
            Length = len;
            IsProxy = false;
            UDPData = GetUdpPack();
        }

        public UdpFrame(int pSeq, UInt16 seq, UInt16 len, UdpCommand cmd, byte[] data, UInt16 proxyID)
        {
            PackageSeq = pSeq;
            Seq = seq;
            Command = cmd;
            Data = data;
            Length = len;
            if (proxyID > 0)
            {
                IsProxy = true;
                ProxyID = proxyID;
            }
            UDPData = GetUdpPack();
        }

        

        public UdpFrame(byte[] data)
        {
            int headLen = HeadLength;
            UDPData = data;
            Command = (UdpCommand)data[0];

            byte[] pid = new byte[4];
            Array.Copy(data, 1, pid, 0, 4);
            PackageSeq = BitConverter.ToInt32(pid, 0);

            byte[] sid = new byte[2];
            Array.Copy(data, 5, sid, 0,2);
            Seq = BitConverter.ToUInt16(sid, 0);

            byte[] len = new byte[2];
            Array.Copy(data, 7, len, 0, 2);
            Length = BitConverter.ToUInt16(len, 0);

            byte[] proxyData = new byte[2];
            Array.Copy(data, 9, proxyData, 0, 2);
            UInt16 proxyId = BitConverter.ToUInt16(proxyData,0);
            if ((proxyId & 0x80) == 0x80)
            {
                IsProxy = true;
                ProxyID = (UInt16)(proxyId & (~(UInt16)0x80));
            }
            

            int l = data.Length - headLen;
            if (l > 0)
            {
                Data = new byte[l];
                Array.Copy(data, headLen, Data, 0, l);
            }
        }

        public byte[] GetUdpPack()
        {
            int headLen = HeadLength;
            int len = headLen;

            if (Data != null && Data.Length > 0)
            {
                len += Data.Length;
            }

            byte[] data = new byte[len];
            data[0] = (byte)Command;

            byte[] packageIDData = BitConverter.GetBytes(PackageSeq);
            packageIDData.CopyTo(data, 1);

            byte[] seqData = BitConverter.GetBytes(Seq);
            seqData.CopyTo(data, 5);

            byte[] lenData = BitConverter.GetBytes(Length);
            lenData.CopyTo(data, 7);

            UInt16 proxyId = 0;
            if (IsProxy)
            {
                proxyId = (UInt16)((UInt16)0x80 | ProxyID);
            }
            
            byte[] pidData = BitConverter.GetBytes(proxyId);
            pidData.CopyTo(data, 9);

            if (Data != null && Data.Length > 0)
            {
                Data.CopyTo(data, headLen);
            }

            return data;
        }
        
    }


    public enum UdpCommand : byte
    {
        Data, //发送数据
        Confirm, //收到确认
        Connect, //连接
        ConnectConfirm, //连接确认
        Close, //关闭
        CloseConfirm, //关闭确认
        UnConnected, //未连接
        Ping,
        Pong,
        Text //发送文本
    }
}
