using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.Threading;
using Dynamic.Core.Log;

namespace Dynamic.Net
{
    public delegate void OnSendPackageCallback(UdpPackage package, EndPoint target, UInt16 proxyId);

    public class UdpSender
    {
        private UdpPackageCache packageCache = new UdpPackageCache();

        public UdpPackageCache PackageCache
        {
            get { return packageCache; }
        }

        public int frameLength = 1000;
        public int FrameLength
        {
            get { return frameLength; }
            set { frameLength = value; }
        }

        public int PerSendFrames { get; set; }

        private TimeSpan frameSendTimeout = TimeSpan.FromSeconds(5);
        public TimeSpan FrameSendTimeout
        {
            get { return frameSendTimeout; }
            set { frameSendTimeout = value; }
        }

        private int frameRetryCount = 3;
        public int FrameRetryCount
        {
            get { return frameRetryCount; }
            set { frameRetryCount = value; }
        }

        private Dictionary<string, long> avgSendCost = new Dictionary<string, long>();


        private List<SendItem> sendList = new List<SendItem>();


      //  private List<SendItem> sendingList = new List<SendItem>();

        public event OnSendPackageCallback SendPackageError;

        public event OnSendPackageCallback SendPackageSuccess;

        private List<SendThread> processerList = new List<SendThread>();

        public UdpSocket Client { get; private set; }

        private bool isSending = false;
        public bool IsSending
        {
            get { return isSending; }
            set { isSending = value; }
        }

        public UdpSender(UdpSocket socket)
        {
            //sendingList = new List<SendItem>();
            sendList = new List<SendItem>();
            Client = socket;
        }

        public void Send(byte[] data, EndPoint target, UInt16 proxyId, Action<UdpPackage, bool> callback)
        {
            UdpPackage p = null;
            lock (packageCache)
            {
                p = packageCache.Push(target, data, frameLength, proxyId);
            }
            p.Callback = callback;
            int c = 0;
            lock (sendList)
            {
                sendList.AddRange(p.Select(x => new SendItem() { Frame = x, Time = DateTime.Now, RetryCount = 0, Target = target }));
                c = sendList.Count;
            }

            lock (processerList)
            {
                processerList.FirstOrDefault(x => x.Notify());
            }

        }


        public void SendText(string text, EndPoint target, UInt16 proxyId, Action<UdpPackage, bool> callback)
        {
            UdpPackage p = null;
            lock (packageCache)
            {
                p = packageCache.PushText(target, text, frameLength, proxyId);
            }
            p.Callback = callback;
            lock (sendList)
            {
                sendList.AddRange(p.Select(x => new SendItem() { Frame = x, Time = DateTime.Now, RetryCount = 0, Target = target }));
            }
            lock (processerList)
            {
                processerList.FirstOrDefault(x => x.Notify());
            }
        }

        public long GetAvgSendCost(EndPoint ep)
        {
            string key = ep.ToString();

            long cost = 0;
            lock (avgSendCost)
            {
                if (avgSendCost.ContainsKey(key))
                {
                    cost = avgSendCost[key];
                    cost = cost + cost / 2;
                }
            }
            if (cost <= 0)
            {
                cost = (long)frameSendTimeout.TotalMilliseconds;
            }

            return cost;
        }

        public void ReceivedFeedback(UdpFrame frame)
        {
            DateTime now = DateTime.Now;
            UdpFrame dataFrame = null;
            lock (packageCache)
            {
                dataFrame = packageCache.ChangeFrameStatus(frame.PackageSeq, frame.Seq);
                
            }
            //EndPoint ep = null;
            //从正在发送的列表中移除
            //lock (sendingList)
            //{
            //    var si = sendingList.FirstOrDefault(x => x.Frame.PackageSeq == frame.PackageSeq && x.Frame.Seq == frame.Seq);
            //    if (si != null)
            //    {
            //        sendingList.Remove(si);
            //        ep = si.Target;
            //    }
            //}

            lock (packageCache)
            {
                UdpPackage up = packageCache.PackageList.FirstOrDefault(x => x.PackageID == frame.PackageSeq);

                if (up != null)
                {
                    long cost = (long)(now - dataFrame.LastSendTime).TotalMilliseconds;
                    string key = up.Target.ToString();
                    lock (avgSendCost)
                    {
                        if (avgSendCost.ContainsKey(key))
                        {
                            avgSendCost[key] = (avgSendCost[key] + cost) / 2;
                        }
                        else
                        {
                            avgSendCost.Add(key, cost);
                        }
                    }
                }

                if (up != null && up.Count(x => !x.IsSended) == 0)
                {
                    //    Logger.Trace("发送数据包成功：{0} {1}", up.PackageID, up.Data.Length);
                    //包发送完成
                    //packageCache.PackageList.Remove(up);
                    RemovePackage(up.PackageID, false);
                    OnSendPackageSuccess(up, up.Target, up[0].ProxyID);
                    if (up.Callback != null)
                    {
                        up.Callback(up, true);
                    }
                }

            }

        }

        public void RemovePackage(int packageSeq, bool isError)
        {
            UdpPackage up = null;
            lock (packageCache)
            {
                up = packageCache.PackageList.FirstOrDefault(x => x.PackageID == packageSeq);
                if (up != null)
                {
                    packageCache.PackageList.Remove(up);
                    up.ForEach(f =>
                    {
                        f.IsRemoved = true;
                    });
                }
            }
            lock (sendList)
            {
                
                sendList.Where(si => si.Frame.PackageSeq == packageSeq).ToList().ForEach(f =>
                {
                    sendList.Remove(f);
                });
            }

            List<SendThread> list = null;
            lock (processerList)
            {
                list = processerList.ToList();
            }
            list.ForEach(p =>
            {
                p.RemovePackageFrames(packageSeq);
            });

            if (isError && up!= null )
            {
                OnSendPackageError(up, up.Target, up[0].ProxyID);
            }
        }

        public void RemoveFrames(DnsEndPoint target, uint proxyId)
        {
            lock (sendList)
            {
                sendList.Where(x => IsEqualTarget(x.Target, target) && x.Frame.ProxyID == proxyId).ToList()
                    .ForEach(si =>
                    {
                        sendList.Remove(si);
                    });
            }


            List<SendThread> list = null;
            lock (processerList)
            {
                list = processerList.ToList();
            }
            list.ForEach(p =>
            {
                p.RemoveFrames(target, proxyId);
            });
        }

        public bool IsEqualTarget(EndPoint ep, DnsEndPoint target)
        {
            if (ep is IPEndPoint)
            {
                IPEndPoint ep2 = ep as IPEndPoint;
                if (ep2.Address.ToString() == target.Host && ep2.Port == target.Port)
                {
                    return true;
                }
            }
            else if (ep is DnsEndPoint)
            {
                DnsEndPoint ep2 = ep as DnsEndPoint;
                if (ep2.Host == target.Host && ep2.Port == target.Port)
                {
                    return true;
                }
            }
            return false;
        }

        internal List<SendItem> GetSendItems(int count)
        {
            List<SendItem> list = null;
            lock (sendList)
            {
                list = new List<SendItem>();
                for (int i = 0; i < count; i++)
                {
                    if (sendList.Count == 0)
                    {
                        break;
                    }

                    SendItem si = sendList[0];
                    list.Add(si);
                 //   ILogger l = LoggerManager.GetLogger("SEND_" + si.Frame.PackageSeq.ToString());
                  //  l.Trace("发送：{0} {1}", si.Frame.PackageSeq, si.Frame.Seq);
                    sendList.Remove(si);
                }
            }

            return list;
        }

        public NetStatistic GetStatisticInfo()
        {
            NetStatistic s = new NetStatistic();
            try
            {
                
                NetStatisticGroup basicGrop = s.AddGroup("基本信息", true);
                lock (sendList)
                {
                    basicGrop.AddItem("队列数", sendList.Count);
                    basicGrop.AddItem("队列字节", sendList.Sum(x => x.Frame.UDPData.Length));
                }

                NetStatisticGroup costGroup = s.AddGroup("平均发送耗时", true);
                costGroup.Columns[0].Unit = "byte";
                lock (avgSendCost)
                {
                    foreach (KeyValuePair<string, long> ci in avgSendCost)
                    {
                        costGroup.AddItem(ci.Key, ci.Value);
                    }
                }
            }
            catch { }
            return s;
        }

        public void Start()
        {
            int threadCount = Math.Max(2, Environment.ProcessorCount);
            List<SendThread> list = null;
            lock (processerList)
            {
                processerList.Clear();
                for (int i = 0; i < threadCount; i++)
                {
                    SendThread st = new SendThread(this, i) ;
                    processerList.Add(st);
                }
                list = processerList.ToList();
            }

            list.ForEach(p =>
            {
                p.Start();
            });

            isSending = true;
        }

        public void Stop()
        {
            isSending = false;
            List<SendThread> list = null;
            lock (processerList)
            {
                list = processerList.ToList();
            }
            list.ForEach(p =>
            {
                p.Stop();
            });
            lock (sendList)
            {
                sendList.Clear();
            }
           
        }

        protected virtual void OnSendPackageError(UdpPackage package, EndPoint target, UInt16 proxyId)
        {
            OnSendPackageCallback h = SendPackageError;
            if (h != null)
            {
                h(package, target, proxyId);
            }
        }

        protected virtual void OnSendPackageSuccess(UdpPackage package, EndPoint target, UInt16 proxyId)
        {
            OnSendPackageCallback h = SendPackageSuccess;
            if (h != null)
            {
                h(package, target, proxyId);
            }
        }

    }


    class SendThread
    {
        public Thread Thread { get; set; }

        public int Index { get; set; }

        private UdpSender sender = null;

        public bool IsWaiting { get; private set; }

        public bool IsRunning { get; private set; }

        private ManualResetEvent dataWait = null;

        private ManualResetEvent stopWait = null;

        private List<SendItem> sendingList = null;

        public object SyncObject = new object();

        ILogger Logger = null;

        public SendThread(UdpSender sender, int index)
        {
            this.sender = sender;
            this.IsWaiting = true;
            this.Index = index;
            Logger = LoggerManager.GetLogger("SENDER_" + index.ToString());
            sendingList = new List<SendItem>();
        }

        public List<SendItem> GetSendingList(int pseq, ushort fseq)
        {
            List<SendItem> list = null;
            lock (sendingList)
            {
                list = sendingList.Where(x => x.Frame.PackageSeq == pseq && x.Frame.Seq == fseq).ToList();
            }

            return list;
        }

        public void RemovePackageFrames(int pseq)
        {
            lock (sendingList)
            {
                sendingList.Where(f => f.Frame.PackageSeq == pseq).ToList().ForEach(f =>
                {
                    sendingList.Remove(f);
                });
            }
        }

        public void RemoveFrames(DnsEndPoint target, uint proxyId)
        {

            lock (sendingList)
            {
                sendingList.Where(x => sender.IsEqualTarget(x.Target, target) && x.Frame.ProxyID == proxyId).ToList()
                    .ForEach(si =>
                    {
                        sendingList.Remove(si);
                    });
            }
        }

        

        public void Start()
        {
            IsRunning = true;
            Thread thread = new Thread(SendProc);
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }


        public bool Notify()
        {
            bool isNotify = false;
            lock (SyncObject)
            {
                if (IsWaiting)
                {
                    dataWait.Set();
                    IsWaiting = false;
                    isNotify = true;
                }
            }
            return isNotify;
        }

        protected virtual void SendProc(object ctx)
        {
            DateTime lastRemoveTimeoutTime = DateTime.Now;
            TimeSpan timeoutCheckInterval = TimeSpan.FromSeconds(1);
            while (IsRunning)
            {
                lock (sendingList)
                {
                    //移除已发送或已移除的帧
                    sendingList.Where(x => x.Frame.IsSended || x.Frame.IsRemoved).ToList().ForEach(si =>
                    {
                        sendingList.Remove(si);
                    });
                }

                if ((DateTime.Now - lastRemoveTimeoutTime) > timeoutCheckInterval)
                {
                    //从Sending List移除超时或已发送的帧
                    List<SendItem> errorList = null;
                    lock (sendingList)
                    {

                        errorList = sendingList.Where(x => x.RetryCount >= sender.FrameRetryCount).ToList();


                    }
                    if (errorList.Count > 0)
                    {
                        errorList.Select(x => x.Frame.PackageSeq).Distinct().ToList().ForEach(pid =>
                        {
                            Logger.Debug("移除超时包：{0}", pid);
                            sender.RemovePackage(pid, true);
                            //由Sender统一移除正在发送的帧
                            //RemovePackageFrames(pid);
                        });
                    }
                    lastRemoveTimeoutTime = DateTime.Now;
                }

               

                int c = 0;
                int c1 = 0;
                int c2= 0;
                List<SendItem> timeoutList = null;
                lock (sendingList)
                {
                    c = sendingList.Count(x => x.RetryCount == 0);
                    c1 = sendingList.Count;
                    if (sendingList.Count > 0)
                    {
                        timeoutList = new List<SendItem>();
                        foreach (SendItem si in sendingList)
                        {
                            long cost=  sender.GetAvgSendCost(si.Target);
                            if (si.LastSendTime != DateTime.MinValue &&
                                (DateTime.Now - si.LastSendTime).TotalMilliseconds >= cost)
                            {
                                si.Timeout = true;
                            }
                            else if (si.LastSendTime != DateTime.MinValue)
                            {
                                si.TimeoutTime = si.LastSendTime.AddMilliseconds(cost);
                            }
                            timeoutList.Add(si);
                        }
                        
                        
                    }
                }

                //if (timeoutList != null && timeoutList.Count > 0)
                //{
                //    c2 = timeoutList.Count;
                //    timeoutList.ForEach(si =>
                //    {
                //        si.Timeout = true;
                //    });
                //}

               // c = sender.PerSendFrames - c;
                c = sender.PerSendFrames - ( c- c2) ;
               // Logger.Trace("添加：{0}", c);
                List<SendItem> siList = sender.GetSendItems(c);

                

                if (siList.Count == 0 && sendingList.Count == 0)
                {
                    lock (SyncObject)
                    {
                        dataWait = new ManualResetEvent(false);
                        IsWaiting = true;
                    }
                    dataWait.WaitOne(TimeSpan.FromSeconds(3));
                    lock (SyncObject)
                    {
                        IsWaiting = false;
                        dataWait = null;
                    }
                    continue;
                }

                List<SendItem> curSendList = new List<SendItem>();
                lock (sendingList)
                {
                    curSendList.AddRange(siList);
                    curSendList.AddRange(sendingList);
                    sendingList.AddRange(siList);
                    //siList.AddRange(sendingList);
                }

                long minWait = -1;
                foreach(SendItem si in curSendList)
                {
                    if (si.Frame.IsSended || si.Frame.IsRemoved)
                    {
                        continue;
                    }
                   
                    if (si.LastSendTime == DateTime.MinValue)
                    {
                        sender.Client.InnerSendFrame(si.Frame, si.Target);
                        si.LastSendTime = DateTime.Now;
                        si.Frame.LastSendTime = DateTime.Now;
                    }
                    else
                    {
                        if( si.Timeout)
                        {
                        //if ((DateTime.Now - si.LastSendTime) >= sender.FrameSendTimeout)
                        //{
                            si.RetryCount++;
                           // Logger.Trace("重试：{0} {1}", si.Frame.PackageSeq, si.Frame.Seq);
                            sender.Client.InnerSendFrame(si.Frame, si.Target);
                            si.LastSendTime = DateTime.Now;
                            si.Frame.LastSendTime = DateTime.Now;
                            si.Timeout = false;
                        }
                        else
                        {
                            if (minWait == -1)
                            {
                                minWait = (long)(si.TimeoutTime - DateTime.Now).TotalMilliseconds;
                            }
                            else
                            {
                                minWait = Math.Min(minWait, (long)(si.TimeoutTime - DateTime.Now).TotalMilliseconds);
                            }
                            
                        }
                    }
                }

                if (siList.Count >= 5 * sender.PerSendFrames)
                {
                    //有太多失败的帧
                    var q = from s in siList
                            where s.RetryCount > 0
                            group s by s.Frame.PackageSeq into g
                            select new
                            {
                                PackageSeq = g.Key,
                                Count = g.Count(),
                                RetryCount = g.Average(x => x.RetryCount)
                            };
                    var errorList = q.ToList().OrderByDescending(x => x.Count).ToList();

                    sender.RemovePackage(errorList[0].PackageSeq, true);
                    Logger.Debug("移除失败过多的包：{0}", errorList[0].PackageSeq);
                    //RemovePackageFrames(errorList[0].PackageSeq);

                    errorList.RemoveAt(0);
                    if (errorList.Count > 0)
                    {
                        errorList = errorList.OrderByDescending(x => x.RetryCount).ToList();
                        sender.RemovePackage(errorList[0].PackageSeq, true);
                        Logger.Debug("移除重试过多的包：{0}", errorList[0].PackageSeq);
                        //RemovePackageFrames(errorList[0].PackageSeq);
                    }

                }


                minWait = Math.Max(10, minWait);
                minWait = Math.Min(minWait, 5000);
                if (minWait < 100)
                {
                    Thread.Sleep((int)minWait);
                }
                else
                {
                    lock (SyncObject)
                    {

                        dataWait = new ManualResetEvent(false);
                        IsWaiting = true;
                    }
                    dataWait.WaitOne((int)minWait);
                    lock (SyncObject)
                    {
                        IsWaiting = false;
                        dataWait = null;
                    }

                }


            }
            lock (SyncObject)
            {
                if (stopWait != null)
                {
                    stopWait.Set();
                    stopWait = null;
                }
            }
            lock (sendingList)
            {
                sendingList.Clear();
            }

        }

        public void Stop()
        {
            try
            {
                stopWait = new ManualResetEvent(false);
                IsRunning = false;
                lock (SyncObject)
                {
                    if (dataWait != null)
                    {
                        dataWait.Set();
                    }
                }
                if (stopWait != null)
                {
                    stopWait.WaitOne(TimeSpan.FromSeconds(2));
                    stopWait = null;
                }
            }
            catch { }
        }


       
    }
}
