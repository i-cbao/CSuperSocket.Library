using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Dynamic.Core.Log;

namespace Dynamic.Net
{
    public class UdpPackageCache
    {
        public List<UdpPackage> PackageList { get; private set; }
        private int packageSeq = 0;

        ILogger logger = LoggerManager.GetLogger("Package");

        public UdpPackageCache()
        {
            PackageList = new List<UdpPackage>();
        }

        public UdpPackage Push(EndPoint target, byte[] data, int frameLength, UInt16 proxyId)
        {
    
            lock (PackageList)
            {
                UdpPackage package = new UdpPackage(data, packageSeq, frameLength, proxyId, target);
                PackageList.Add(package);
                if (packageSeq >= (int.MaxValue - 1))
                {
                    packageSeq = 0;
                }
              //  logger.Trace("包ID： {0}", packageSeq);
                packageSeq++;
                return package;
            }
        }


        public UdpPackage PushText(EndPoint target, string text, int frameLength, UInt16 proxyId)
        {
            byte[] data = new byte[0];
            if (!String.IsNullOrEmpty(text))
            {
                data = Encoding.UTF8.GetBytes(text);
            }
            lock (PackageList)
            {
                UdpPackage package = new UdpPackage(data, packageSeq, frameLength, proxyId, target, true);
                PackageList.Add(package);
                if (packageSeq >= (int.MaxValue - 1))
                {
                    packageSeq = 0;
                }
                packageSeq++;
                return package;
            }
        }

        public UdpFrame ChangeFrameStatus(int packageID, UInt16 seq)
        {
            UdpPackage p = null;
            UdpFrame f = null;
            lock (PackageList)
            {
                p = PackageList.FirstOrDefault(x => x.PackageID == packageID);

                if (p != null)
                {

                    f = p.FirstOrDefault(x => x.Seq == seq);

                    f.IsSended = true;
                }
            }

            return f;

        }
    }
}
