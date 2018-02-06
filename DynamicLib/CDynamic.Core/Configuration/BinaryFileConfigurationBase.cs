using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Core.Configuration
{
    /// <summary>
    /// 使用二进制序列化的配置基类
    /// </summary>
#if !NETCOREAPP
  [Serializable]
#endif
    public abstract class BinaryFileConfigurationBase : ConfigurationBase
    {
        public BinaryFileConfigurationBase()
            : base(new ConfigurationBinaryFileRepository())
        {
        }
    }
}
