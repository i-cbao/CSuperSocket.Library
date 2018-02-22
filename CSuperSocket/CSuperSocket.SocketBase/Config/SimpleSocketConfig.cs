using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CSuperSocket.SocketBase.Config
{
    public class SimpleSocketConfig
    {
        public SimpleSocketConfig()
        {
            this.Isolation = IsolationMode.None;
            this.TextEncoding = "UTF-8";
            this.Mode = SocketMode.Tcp;
            this.Ip = "Any";
            this.Port = 7000;
            this.ListenBacklog = 2147483647;
            this.MaxConnectionNumber = 65535;
            this.IdleSessionTimeOut = 300;
            this.ClearIdleSessionInterval = 60;
            this.ClearIdleSession = true;
            this.ReceiveBufferSize = 128;
            this.SendBufferSize = 128;
            this.DisableSessionSnapshot = true;
            this.Name = "IcbTerminal";
            this.MaxCompletionPortThreads = 10000;
            this.PerformanceDataCollectInterval = 120;
            this.DisablePerformanceDataCollector = false;
            this.ServerTypeNiceName = "IcbTerminalService";
            this.ServerTypeFullName = "ICheBao.Service,ICheBao.Protocols";
            this.KeepAliveTime = 10;

            Console.WriteLine("运行平台:{0}", RuntimeInformation.OSDescription);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                this.PlatformType = PlatformType.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.PlatformType = PlatformType.Windows;
            }
        }

        public IsolationMode Isolation
        {
            get; set;
        }
        public string TextEncoding { get; set; }
        public SocketMode Mode { get; set; }
        public int KeepAliveTime { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public string ServerTypeNiceName { get; set; }
        public int MaxCompletionPortThreads { get; set; }
        public int PerformanceDataCollectInterval { get; set; }
        public bool DisablePerformanceDataCollector { get; set; }
        public string ServerTypeFullName { get; set; }
        public int ListenBacklog { get; set; }
        public int MaxConnectionNumber { get; set; }
        public int IdleSessionTimeOut { get; set; }
        public int ClearIdleSessionInterval { get; set; }
        public bool ClearIdleSession { get; set; }
        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public bool DisableSessionSnapshot { get; set; }
        public string Name { get; set; }

        public PlatformType PlatformType { get; set; }


    }
}
