using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using Dynamic.Net.KcpSharp.SeqCtrl;
using Dynamic.Core.Log;

namespace Dynamic.Net.KcpSharp.SeqCtrl
{

    public delegate byte[] OpernateFilterBytesHandler(byte[] oriBytes);
}
namespace Dynamic.Net.KcpSharp.NetWork
{
    public abstract class SocketNetworkDriver : IDisposable
    {
        protected readonly ILogger _Logger = LoggerManager.GetLogger("SocketNetworkDriver");
        #region 单例模式实现

        public OpernateFilterBytesHandler UnWrapFrameDataHandler { get; set; }

        public OpernateFilterBytesHandler WrapFrameDataHandler { get; set; }

       protected static readonly Dictionary<string, SocketNetworkDriver> UND =
            new Dictionary<string, SocketNetworkDriver>();

        protected bool IsDisposed = false;

        private Thread receiveTH = null;

        public bool IsServer { get; set; }

        public event EventHandler<SocketAsyncEventArgs> OnReciviceHeartPacket;

        /// <summary>
        /// 获取网络数据适配器
        /// </summary>
        /// <param name="port">网络端口</param>
        /// <param name="isServer">是否是服务器调用</param>
        /// <returns>网络适配器</returns>
        public static   SocketNetworkDriver GetDriver(int port, bool isServer)
        {
            if (!UND.ContainsKey(port.ToString()))
            {
                return null;
            }
            return UND[port.ToString()]; ;
        }
        #endregion

        #region 字段
        /// <summary>
        /// 缓冲区管理器
        /// </summary>
        private BufferManager bufferManager;

        /// <summary>
        /// 用于监听的套接字
        /// </summary>
        protected Socket listenSocket;

        /// <summary>
        /// 当前正常处理的连接数
        /// </summary>
        public int numConnectedSockets { get;protected set; }

        /// <summary>
        /// 最大可并发处理连接数
        /// </summary>
        public int numMaxConnections { get; protected set; }

        /// <summary>
        /// 单个连接处理所需的缓冲区块数量，分为 读 写
        /// </summary>
        private const int opsToPreAlloc = 2;

        /// <summary>
        /// 异步套接字操作池
        /// </summary>
        private SocketAsyncEventArgsPool readWritePool;

        /// <summary>
        /// 控制服务器所接收的链接数
        /// </summary>
        private Semaphore semaphoreAcceptedClients;
        /// <summary>
        /// 是否已启用UDP接口
        /// </summary>
        private bool _started = false;
        #endregion

        /// <summary>
        /// 从网络接收到数据
        /// </summary>
        public event ReceiveDataEventHandler ReceiveData;

        #region 私有方法
        /// <summary>
        /// UDP网络驱动构造函数
        /// </summary>
        /// <param name="port">监听端口</param>
        /// <param name="isServer">是否是服务器调用（服务器调用时，弱端口被占用则不会自动变更）</param>
        protected SocketNetworkDriver(int port, bool isServer)
        {
            Init();
            InitSocket(port, isServer);
            IsServer = isServer;
        }

        /// <summary>
        /// 初始化变量
        /// </summary>
        private void Init()
        {
            this.numConnectedSockets = 0;
            if(this.numMaxConnections < 1000) this.numMaxConnections = 1000;


            //初始化缓冲区管理器
            this.bufferManager = new BufferManager(1024 * numMaxConnections * opsToPreAlloc,
                1024);
            //初始化异步并发套接字操作池
            this.readWritePool = new SocketAsyncEventArgsPool(numMaxConnections);
            //初始化服务器连接接收控制单元
            this.semaphoreAcceptedClients = new Semaphore(numMaxConnections, numMaxConnections);
            //初始化缓冲区
            this.bufferManager.InitBuffer();

            //初始化异步并发处理池
            SocketAsyncEventArgs readWriteEventArg;

            for (Int32 i = 0; i < this.numMaxConnections; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                this.bufferManager.SetBuffer(readWriteEventArg);
                this.readWritePool.Push(readWriteEventArg);
            }
        }
        /// <summary>
        ///子类继承此方法来实例化socket（如：tcp/udp）
        /// </summary>
        /// <param name="localEndPoint"></param>
        /// <returns></returns>
        protected abstract Socket NewSocket(IPEndPoint localEndPoint);

        //初始化Socket
        private void InitSocket(int port, bool isServer)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
           
            this.listenSocket = NewSocket(localEndPoint);
            //构造监听套接字放到子类去
            //this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Dgram,
            //    ProtocolType.Udp);

            bool success = false;
            while (!success)
            {
                try
                {
                   
                    success = true;
                }
                catch (Exception ex)
                {
                    if (!isServer)
                    {
                        localEndPoint.Port += 1;
                        if (localEndPoint.Port >= 65535)
                        {
                            success = true;
                            throw ex;
                        }
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// I/O处理完成
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">异步套接字操作对象</param>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            // 根据完成的操作类型选择处理方式
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    this.ProcessReceive(e);
                    break;
                default:
                    break;
            }
        }
        //接收到数据操作
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                try
                {
                    IPEndPoint remoteIP = e.RemoteEndPoint as IPEndPoint;
                    if (e.BytesTransferred > 0)
                    {
                        //IPEndPoint remoteIP = e.RemoteEndPoint as IPEndPoint;
                        byte[] receiveBuffer = new byte[e.BytesTransferred];
                        System.Buffer.BlockCopy(e.Buffer, e.Offset, receiveBuffer,
                            0, e.BytesTransferred);

                       
                        if (this.UnWrapFrameDataHandler != null && receiveBuffer != null)
                        {
                            byte[] reciceBytes = null;
                            reciceBytes = this.UnWrapFrameDataHandler(receiveBuffer);
                            OnReceiveData(new ReceiveDataEventArgs(reciceBytes, remoteIP));
                        }
                        else
                        {
                            OnReceiveData(new ReceiveDataEventArgs(receiveBuffer, remoteIP));
                        }

                       
                    }
                    else
                    {
                        //空包作为心跳包
                        if (this.OnReciviceHeartPacket != null)
                        {
                            this.OnReciviceHeartPacket.Invoke(this, e);
                        }
                        if (IsServer)
                        {
                            //服务器直接回传心跳包
                            this.SendBuffer(new byte[0], remoteIP);

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }


            // 将该连接请求添加到并发处理池中
            this.readWritePool.Push(e);
            // 释放一个当前服务器接收数量，允许服务器继续接收连接
            this.semaphoreAcceptedClients.Release();
        }
        protected abstract bool AsyReceive(SocketAsyncEventArgs saea);
        //开始接受
        private void BeginReceive()
        {
            while (_started)
            {
                this.semaphoreAcceptedClients.WaitOne();
                SocketAsyncEventArgs saea = this.readWritePool.Pop();
                bool rec = AsyReceive(saea);
                if (!rec)
                {
                    this.ProcessReceive(saea);
                }
            }
        }

        #endregion

        #region 公有方法
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="cell">数据包对象</param>
        /// <param name="remoteIP">要发送到的目标网络节点</param>
        public void Send(SendCell cell, IPEndPoint remoteIP)
        {
            if (_started)
            {
                byte[] buffer = cell.ToBuffer();
                SendInternal(buffer, remoteIP);
            }
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        public void Start()
        {
            if (!_started)
            {
                _started = true;
                receiveTH = new Thread(new ThreadStart(BeginReceive));
                receiveTH.IsBackground = true;
                receiveTH.Start();
            }
        }
        /// <summary>
        /// 关闭监听
        /// </summary>
        public void Stop()
        {
            _started = false;

            if (receiveTH != null)
            {
                if (receiveTH.IsAlive)
                {
                    try
                    {
                        receiveTH.Abort();
                    }
                    catch (ThreadStateException ex)
                    {

                    }
                }
                receiveTH = null;
            }
        }
        public void SendBuffer(byte[] buffer, IPEndPoint remoteIP)
        {
            this.SendInternal(buffer, remoteIP);
        }

        //发送数据
        protected void SendInternal(byte[] buffer, IPEndPoint remoteIP)
        {
            if (this.WrapFrameDataHandler != null && buffer != null)
            {
                buffer = this.WrapFrameDataHandler(buffer);
            }
            this.listenSocket.SendTo(buffer, remoteIP);
        }
        #endregion

        //触发接收到数据事件
        protected virtual void OnReceiveData(ReceiveDataEventArgs edea)
        {
            try
            {
                if (edea == null) return;
                if (ReceiveData != null)
                {
                    ReceiveData(this, edea);
                }
            }
            catch (SocketException sEx)
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString() + "_结束处理数据_\n错误数据:"
                       + sEx.Message + "\n堆栈消息:" + sEx.StackTrace + "\n错误源:" + sEx.Source);
            }
            catch
            {
                throw;
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Stop();
                IsDisposed = true;
                //手动释放了垃圾，不用垃圾回收器调用
                GC.SuppressFinalize(this);
            }
        }
    }
}
