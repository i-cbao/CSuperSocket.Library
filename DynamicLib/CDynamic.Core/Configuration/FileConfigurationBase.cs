using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Core.Configuration
{
    /// <summary>
    /// 基于文件系统的配置类
    /// </summary>
    [Serializable]
    public abstract class FileConfigurationBase : ConfigurationBase
    {
        public FileConfigurationBase()
            : base(new ConfigurationFileRepository())
        {
        }
    }
}
