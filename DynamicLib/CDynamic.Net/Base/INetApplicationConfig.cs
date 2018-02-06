using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Dynamic.Net.Base
{
    public interface INetApplicationConfig
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 服务配置
        /// </summary>
        List<INetServerConfig> ServerConfig { get; set; }
    }
}
