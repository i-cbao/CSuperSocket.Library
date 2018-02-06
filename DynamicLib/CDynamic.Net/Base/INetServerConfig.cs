using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Dynamic.Net.Base
{
    public interface INetServerConfig
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        NetServerType ServerType { get; set; }

        /// <summary>
        /// 监听寻址方案
        /// </summary>
        AddressFamily AddressFamily { get; set; }

        /// <summary>
        /// 监听地址
        /// </summary>
        IPAddress Address { get; set; }

        /// <summary>
        /// 监听端口
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// 挂起连接队列的最大长度
        /// </summary>
        int Backlog { get; set; }

        /// <summary>
        /// 最大连接数
        /// </summary>
        int MaxConnectionNumber { get; set; }

        /// <summary>
        /// 超时时间，分钟
        /// </summary>
        long SessionTimeout { get; set; }
    }
}
