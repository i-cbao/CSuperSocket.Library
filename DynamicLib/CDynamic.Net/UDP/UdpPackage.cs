using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using Dynamic.Core.Log;

namespace Dynamic.Net
{
    public class UdpPackage : List<UdpFrame>
    {
        public int PackageID { get; private set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastTime { get; set; }

        public byte[] Data { get; set; }

        public Action<UdpPackage, bool> Callback { get; set; }

        public EndPoint Target { get; set; }

        ILogger logger = LoggerManager.GetLogger("Package");

        public UdpPackage(int packageID)
        {
            PackageID = packageID;
            CreateTime = DateTime.Now;
        }

        public UdpPackage(byte[] data, int packageID, int len, UInt16 proxyId, EndPoint target)
            : this(data, packageID, len, proxyId, target, false)
        {
        }

        public UdpPackage(byte[] data, int packageID, int len, UInt16 proxyId,EndPoint target, bool isText)
        {
            UInt16 fseq = 0;
            Data = data;
            Target = target;
            CreateTime = DateTime.Now;
            PackageID = packageID;
            UdpCommand cmd = UdpCommand.Data;
            if (isText)
            {
                cmd = UdpCommand.Text;
            }
            if (data == null || data.Length == 0)
            {

                UdpFrame frame = new UdpFrame(packageID, fseq, 0, cmd, data, proxyId);
                Add(frame);
            }
            else
            {
                int curPos = 0;
                UInt16 flen = (UInt16)Math.Ceiling((double)data.Length / (double)len);
                while (true)
                {
                    int l = len;
                    if ((curPos + l) >= data.Length)
                    {
                        l = data.Length - curPos;
                    }
                    byte[] fdata = new byte[l];
                    Array.Copy(data, curPos, fdata, 0, l);
                    UdpFrame frame = new UdpFrame(packageID, fseq, flen, cmd, fdata, proxyId);
                    Add(frame);
                    fseq++;

                    curPos += len;
                    if (curPos >= data.Length)
                    {
                        break;
                    }
                }


                //UdpFrame f = this[this.Count - 1];
                //if (f.Data != null)
                //{
                //    logger.Trace("最后一帧大小：{0} {1} {2}", f.PackageSeq, f.Seq, f.Data.Length);
                //}
                
            }

        }


        public byte[] GetData()
        {
            List<UdpFrame> frameList = this.OrderBy(x => x.Seq).ToList();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            foreach (UdpFrame f in frameList)
            {

                if (f.Data != null && f.Data.Length > 0)
                {
                    bw.Write(f.Data);
                }
            }
            bw.Flush();
            ms.Flush();
            return ms.ToArray();
        }

        public bool IsText()
        {
            if (this.Count == 0)
            {
                return false;

            }
            return this[0].Command == UdpCommand.Text;
        }

    }
}
