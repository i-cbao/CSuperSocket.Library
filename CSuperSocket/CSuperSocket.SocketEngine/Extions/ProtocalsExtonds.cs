using CSuperSocket.SocketBase.Config;
using CSuperSocket.SocketEngine.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSuperSocket.SocketEngine.Extions
{
    public static class ProtocalsExtonds
    {
        public static SocketServiceConfig ConvertSocketServiceCfg(this SimpleSocketConfig simpleSocketConfig)
        {
            SocketServiceConfig ssCfg = null;
            if (simpleSocketConfig != null)
            {
                var serverCfg = new Server();
                serverCfg.KeepAliveTime = 10;
                serverCfg.Listeners = new List<Listener>() {
                new Listener(){
                    Ip="Any",
                    Port=simpleSocketConfig.Port,
                }
            };
                serverCfg.TextEncoding = simpleSocketConfig.TextEncoding;
                serverCfg.Mode = simpleSocketConfig.Mode;
                serverCfg.ServerTypeName = simpleSocketConfig.ServerTypeNiceName;
                serverCfg.ListenBacklog = simpleSocketConfig.ListenBacklog;
                serverCfg.MaxConnectionNumber = simpleSocketConfig.MaxConnectionNumber;
                serverCfg.IdleSessionTimeOut = simpleSocketConfig.IdleSessionTimeOut;
                serverCfg.ClearIdleSessionInterval = simpleSocketConfig.ClearIdleSessionInterval;
                serverCfg.ClearIdleSession = simpleSocketConfig.ClearIdleSession;
                serverCfg.ReceiveBufferSize = simpleSocketConfig.ReceiveBufferSize;
                serverCfg.SendBufferSize = simpleSocketConfig.SendBufferSize;
                serverCfg.DisableSessionSnapshot = simpleSocketConfig.DisableSessionSnapshot;
                serverCfg.Name = simpleSocketConfig.Name;
                serverCfg.PlatformType = simpleSocketConfig.PlatformType;
                


                ssCfg = new SocketServiceConfig(serverCfg);
                ssCfg.MaxCompletionPortThreads = simpleSocketConfig.MaxCompletionPortThreads; ;
                ssCfg.PerformanceDataCollectInterval = simpleSocketConfig.PerformanceDataCollectInterval;
                ssCfg.DisablePerformanceDataCollector = simpleSocketConfig.DisablePerformanceDataCollector;
                ssCfg.Isolation = simpleSocketConfig.Isolation;

                ssCfg.ServerTypes = new List<TypeProvider>();
                ssCfg.ServerTypes.Add(new TypeProvider()
                {
                    Name = simpleSocketConfig.ServerTypeNiceName,
                    Type = simpleSocketConfig.ServerTypeFullName
                });
            }
            return ssCfg;
        }
    }
}
