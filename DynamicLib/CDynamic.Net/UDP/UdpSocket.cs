using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Dynamic.Core.Log;

namespace Dynamic.Net
{
    public abstract class UdpSocket
    {
        protected virtual ILogger Logger { get; set; }
       // private UdpPackageCache packageCache = new UdpPackageCache();
       // private UdpPackageCache receivedCache = new UdpPackageCache();
        private List<UdpReceivedItem> receivedCache = new List<UdpReceivedItem>();
        private List<UdpReceivedItem> processedPackage = new List<UdpReceivedItem>();
        //private List<SendItem> sendList = new List<SendItem>();
        //当前正在发送的
        //private List<SendItem> sendingList = new List<SendItem>();

        //private ManualResetEvent sendWait = null;
       // private TimeSpan frameSendTimeout = TimeSpan.FromSeconds(5);
      //  private int frameRetryCount = 3;
      //  private int frameLength = 1000;
        private List<IFrameWrapper> frameWrapper = null;
        private int port = 0;

       // private bool isSending = false;
       // System.Threading.ManualResetEvent stopWait = null;
        private object locker = new object();

        public event EventHandler<ReceivedDataEventArgs> ReceivedData;

        protected bool isReceiving = false;

        private long maxProxyID = 0;

        /// <summary>
        /// 每次发送多少个帧
        /// </summary>
       // public int PerSendFrames { get; set; }

        public virtual DateTime LastReceivedTime { get; protected set; }

        protected UdpSender Sender { get; set; }

        private DateTime lastCheckReceivTimeoutTime = DateTime.MinValue;

        public UdpSocket()
        {
            frameWrapper = new List<IFrameWrapper>();
           // PerSendFrames = 20;
            Sender = new UdpSender(this);
            Sender.PerSendFrames = 20;
            Sender.FrameLength = 1000;
            Sender.FrameRetryCount = 3;
            SendTimeout = TimeSpan.FromSeconds(15);
            ReceiveTimeout = TimeSpan.FromSeconds(300); //5分钟
            Sender.FrameSendTimeout = TimeSpan.FromSeconds(5);
            Logger = LoggerManager.GetLogger("UdpSocket");
        }

        public int PerSendFrames
        {
            get { return Sender.PerSendFrames; }
            set { Sender.PerSendFrames = value; }
        }

        public int FrameLength
        {
            get { return Sender.FrameLength; }
            set { Sender.FrameLength = value; }
        }

        public int MaxRetry
        {
            get { return Sender.FrameRetryCount; }
            set { Sender.FrameRetryCount = value; }
        }

        public TimeSpan FrameSendTimeout
        {
            get { return Sender.FrameSendTimeout; }
            set { Sender.FrameSendTimeout = value; }
        }
        public TimeSpan SendTimeout
        {
            get;
            set;
        }

        public TimeSpan ReceiveTimeout
        {
            get;
            set;
        }

        protected bool IsSending
        {
            get { return Sender.IsSending; }
            set { Sender.IsSending = value; }
        }

        public bool IsRunning
        {
            get { return isReceiving; }
        }

        public int Port
        {
            get { return port; }
            protected set { port = value; }
        }


        protected bool IsReceiving
        {
            get { return isReceiving; }
            set { isReceiving = value; }
        }

        public EndPoint Target { get; protected set; }

        public List<IFrameWrapper> FrameWrapper
        {
            get { return frameWrapper; }
        }

        public UInt16 GetProxyID()
        {
            UInt16 pid = 0;
            pid =(UInt16) Interlocked.Increment(ref maxProxyID);

            return pid;
        }

        public abstract void Close();
      

        protected UdpClient Client
        {
            get;
            set;
        }

        protected virtual void StartSend()
        {
            Sender.Start();
        }

        
        protected virtual void StopSend()
        {
            Sender.Stop();
            
        }

        protected virtual void StartReceive()
        {
            UdpClient c = Client;
            if (isReceiving || c == null )
            {
                return;
            }
            isReceiving = true;
            try
            {
               
                c.BeginReceive(this.receiveProc, c);
            }
            catch (SocketException e)
            {
                Logger.Error("接收数据异常：\r\n{0}", e.ToString());
                isReceiving = false;
                OnReceiveError(e);
                throw;
            }
            catch (ObjectDisposedException e)
            {
                isReceiving = false;
                Logger.Error("接收数据异常：\r\n{0}", e.ToString());
                OnReceiveError(e);
                throw;
            }
        }


        protected virtual void beginReceive(UdpClient c)
        {
            if (!isReceiving)
                return;

            try
            {
                c.BeginReceive(this.receiveProc, c);
            }
            catch (SocketException e)
            {
                Logger.Debug("接收数据发生异常：\r\n{0}", e.ToString());
                if (e.SocketErrorCode == SocketError.ConnectionReset ||
                    e.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    Logger.Debug("继续接收");
                    beginReceive(c);
                    return;
                }
            }
            catch (ObjectDisposedException e)
            {
                isReceiving = false;
                OnReceiveError(e);

            }
        }

        protected virtual void StopReceive()
        {
            isReceiving = false;
        }

        protected void RemoveCachePackage(IPEndPoint target)
        {
            lock (receivedCache)
            {
                var rl = processedPackage.Where(x =>x.Source.Address.Equals( target.Address) && x.Source.Port == target.Port).ToList();
                rl.ForEach(p =>
                {
                    processedPackage.Remove(p);
                });
            }
        }

        protected void RemoveSendQueue(DnsEndPoint target, uint proxyId)
        {
            Sender.RemoveFrames(target, proxyId);
            
        }

        protected virtual void receiveProc(IAsyncResult result)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any,0);
            UdpClient client = result.AsyncState as UdpClient;
            byte[] data = null;
            if (client == null)
            {
                Logger.Trace("退出接收0");
                return;
            }
            try
            {

                data = client.EndReceive(result, ref ep);
            }
            catch (SocketException e)
            {
                Logger.Debug("接收数据异常：\r\n{0}", e.ToString());
                beginReceive(client);
                //OnReceiveError(e);
                //isReceiving = false;
                return;
            }
            catch (ObjectDisposedException e)
            {
                Logger.Trace("退出接收1");
                Client = null;
                return;
            }
            catch (NullReferenceException e)
            {
                Logger.Trace("退出接收2");
                Client = null;
                return;
            }
            if (data == null && isReceiving)
            {
                beginReceive(client);
                return;
            }
            if (!isReceiving)
            {
                Logger.Trace("退出接收3");
                return;
            }

            byte[] rdata = new byte[data.Length];
            Buffer.BlockCopy(data, 0, rdata, 0, data.Length);
            UdpClient c = Client;
            if (isReceiving && c != null)
            {
                beginReceive(c);
            }

            UdpFrame frame = InnerReceivedFrameData(rdata, ep);

            if (frame != null)
            {
                if (frame.Command == UdpCommand.Data || frame.Command == UdpCommand.Text)
                {
                    UdpFrame confirm = new UdpFrame(frame.PackageSeq, frame.Seq, frame.Length, UdpCommand.Confirm, null, frame.ProxyID);
                    InnerSendFrame(confirm, ep);
                   // ILogger l = LoggerManager.GetLogger("R_" + frame.PackageSeq.ToString());
                  //  l.Trace(String.Format("收到数据帧：{0} {1} {2} ", frame.PackageSeq, frame.Seq, frame.Command));
                    lock (receivedCache)
                    {
                       // Logger.Trace(String.Format("收到数据帧：{0} {1} {2} ", frame.PackageSeq, frame.Seq, frame.Command));

                        //是否已经处理
                        UdpReceivedItem ignoreItem = processedPackage.FirstOrDefault(x => x.PackageSeq == frame.PackageSeq && x.Source.Address.Equals( ep.Address) && x.Source.Port == ep.Port && x.ProxyID == frame.ProxyID);
                        if (ignoreItem == null)
                        {
                            UdpReceivedItem rItem = receivedCache.FirstOrDefault(x => x.PackageSeq == frame.PackageSeq && x.Source.Address.Equals(ep.Address) && x.Source.Port == ep.Port && x.ProxyID == frame.ProxyID);
                            if (rItem == null)
                            {
                                rItem = new UdpReceivedItem();
                                rItem.Source = ep;
                                rItem.ProxyID = frame.ProxyID;
                                rItem.PackageSeq = frame.PackageSeq;
                                rItem.Package = new UdpPackage(frame.PackageSeq);
                                rItem.LastTime = DateTime.Now;
                                rItem.Package.LastTime = DateTime.Now;
                                receivedCache.Add(rItem);
                            }

                            UdpFrame old = rItem.Package.FirstOrDefault(x => x.Seq == frame.Seq);
                            if (old == null)
                            {

                                rItem.Package.Add(frame);
                                rItem.Package.LastTime = DateTime.Now;
                                rItem.LastTime = DateTime.Now;
                                if (frame.Length == rItem.Package.Count)
                                {
                                    //接收完成
                                    receivedCache.Remove(rItem);
                                    bool isText = rItem.Package.IsText();
                                    Byte[] packageData = rItem.Package.GetData();
                                   
                                    ThreadPool.QueueUserWorkItem(new WaitCallback((cr) =>
                                    {
                                        OnReceivedData(packageData, ep, rItem.ProxyID, isText);
                                    }));
                                    rItem.Package = null;
                                    rItem.LastTime = DateTime.Now;
                                    processedPackage.Add(rItem);

                                    var rl = processedPackage.Where(x => (DateTime.Now - x.LastTime).TotalMilliseconds >= (SendTimeout.TotalMilliseconds * 3)).ToList();
                                    rl.ForEach(x => processedPackage.Remove(x));

                                }
                            }
                        }


                    }


                    //add  whb 2015-12-22 移除接收超时的包  防止缺损帧的包长期占用内存，导致内存不断上升的问题
                    //3分钟检查一次
                    if ((DateTime.Now - lastCheckReceivTimeoutTime).TotalMinutes >= 3)
                    {
                        lastCheckReceivTimeoutTime = DateTime.Now;
                        lock (receivedCache)
                        {
                            var removeList = receivedCache.Where(x => (DateTime.Now - x.LastTime) >= ReceiveTimeout).ToList();
                            removeList.ForEach(r =>
                            {
                                receivedCache.Remove(r);
                            });
                        }
                    }


                }
                else if (frame.Command == UdpCommand.Confirm)
                {
                    //lock (sendedList)
                    //{
                    //    sendedList.Add(frame);
                    //}
                    ReceivedFeedback(frame);
                }
            }

           
            
        }


        public virtual NetStatistic GetStatisticInfo()
        {
            NetStatistic s = Sender.GetStatisticInfo();
            NetStatisticGroup g = s.AddGroup("接收缓存", true);
            lock (receivedCache)
            {
                g.AddItem("接收缓存包总数", receivedCache.Count);
                g.AddItem("已处理包数", processedPackage.Count);
            }


            return s;

        }

        protected virtual void OnReceiveError(Exception exception)
        {

        }

        protected virtual void ReceivedFeedback(UdpFrame frame)
        {
            Sender.ReceivedFeedback(frame);

        }

        /// <summary>
        /// 发送UDP数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="target">目标</param>
        protected virtual void Send(byte[] data, EndPoint target, UInt16 proxyId, Action<UdpPackage,bool> callback)
        {
            Sender.Send(data, target, proxyId, callback);

        }


        protected virtual void SendText(string text, EndPoint target, UInt16 proxyId, Action<UdpPackage, bool> callback)
        {
            Sender.SendText(text, target, proxyId, callback);
        }



        /// <summary>
        /// 发送帧，帧不会重试，仅发送一次
        /// </summary>
        /// <param name="frame">数据帧</param>
        /// <param name="target">目标</param>
        protected virtual void SendFrame(UdpFrame frame, EndPoint target)
        {
            //直接发送帧命令
            InnerSendFrame(frame, target);

        }


       

   
        protected virtual void OnSendPackageError(UdpPackage package, EndPoint target, UInt16 proxyId)
        {
        }

        protected virtual void OnSendedPackage(UdpPackage package, EndPoint target, UInt16 proxyId)
        {

        }

        //public List<UdpFrame> GetDataSendFrames()
        //{
        //    return sendedList.ToList();
        //}

       // private List<UdpFrame> sendedList = new List<UdpFrame>();
        internal  void InnerSendFrame(UdpFrame frame, EndPoint target)
        {
            if (frame.Command == UdpCommand.Connect)
            {
                Logger.Trace("发送连接帧命令：{0}", target);
            }
            else if (frame.Command == UdpCommand.Data)
            {

                //lock (sendedList)
                //{
                //    sendedList.Add(frame);
                //}

               //Logger.Trace("发送数据包帧：{0} {1}", frame.PackageSeq, frame.Seq);
            }
            byte[] data = frame.UDPData;
            if (frameWrapper != null)
            {
                for (int i = 0; i < frameWrapper.Count; i++)
                {
                    data = frameWrapper[i].Wrapper(frame.UDPData, frame.Command);
                }

            }
            //if (frame.Command == UdpCommand.Data && frame.Length == (frame.Seq + 1))
            //{
            //    Logger.Trace("最后一帧：{0} {1} {2} {3}", frame.PackageSeq, frame.Seq, frame.Data.Length, data.Length);
            //    Logger.Trace("数据 :\r\n{0}", BitConverter.ToString(frame.Data));
            //}

            SendData(data, target, frame.ProxyID);

        }

        

        private void sendEndProc(IAsyncResult r)
        {
            try
            {
                UdpClient c1 = r.AsyncState as UdpClient;

                c1.EndSend(r);
            }
            catch (Exception e)
            {
                Logger.Error("发送数据异常：{0}", e.ToString());
            }
        }

    

        /// <summary>
        /// 发送封装后的UDP协议数据
        /// </summary>
        /// <param name="data">UDP协议数据</param>
        /// <param name="target">目标</param>
        protected virtual void SendData(byte[] data, EndPoint target, UInt16 proxyId)
        {
            UdpClient client = Client;
            if (client == null || !Sender.IsSending || client.Client == null)
            {
                return;
            }

            //Debug.WriteLine("发送数据");

            if (target is IPEndPoint)
            {

                client.BeginSend(data, data.Length, target as IPEndPoint, new AsyncCallback(sendEndProc), client);

                //client.Send(data, data.Length, target as IPEndPoint);
            }
            else if (target is DnsEndPoint)
            {
                DnsEndPoint ep = target as DnsEndPoint;
                try
                {
                    client.BeginSend(data, data.Length, ep.Host, ep.Port, new AsyncCallback(sendEndProc), client);
                    //client.Send(data, data.Length, ep.Host, ep.Port);
                }
                catch (Exception e)
                {
                    //    LoggerManager.GetLogger("Error").Error(e.ToString());

                    //    if (data == null)
                    //    {
                    //        LoggerManager.GetLogger("Error").Error("data is null");
                    //    }
                    //    if (client == null)
                    //    {
                    //        LoggerManager.GetLogger("Error").Error("client is null");
                    //    }
                    //    if (ep != null)
                    //    {
                    //        LoggerManager.GetLogger("Error").Error(ep.ToString() + " " + data.Length.ToString());
                    //    }
                    //    if (String.IsNullOrEmpty(ep.Host))
                    //    {
                    //        LoggerManager.GetLogger("Error").Error("host is null");
                    //    }

                    //   throw;
                }
            }


        }

        protected virtual UdpFrame InnerReceivedFrameData(byte[] data, IPEndPoint ep)
        {
            if (data == null)
            {
                return null;
            }
            LastReceivedTime = DateTime.Now;
            byte[] frameData = data;
            if (frameWrapper != null)
            {
                for (int i = 0; i < frameWrapper.Count; i++)
                {
                    frameData = frameWrapper[i].UnWrapper(frameData);
                }
            }

            if (data.Length < UdpFrame.HeadLength)
            {
                return null;
            }


            return OnReceivedFrameData(frameData, ep);
        }

        protected virtual UdpFrame OnReceivedFrameData(byte[] data, IPEndPoint ep)
        {
            if (data == null || data.Length < UdpFrame.HeadLength)
            {
                return null;
            }
            
            UdpFrame frame = new UdpFrame(data);
            return frame;
        }

        protected abstract void OnReceivedData(byte[] data, IPEndPoint ep, UInt16 proxyId, bool isText);

        protected  void OnReceivedData(ReceivedDataEventArgs args)
        {
            EventHandler<ReceivedDataEventArgs> h = ReceivedData;
            if (h != null)
            {
                h(this, args);
            }
        }
    }
}
