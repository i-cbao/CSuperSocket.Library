using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Core.Configuration
{
    /// <summary>
    /// 配置仓储接口
    /// </summary>
    public interface IConfigurationRepository
    {
        /// <summary>
        /// 将配置对象保存到仓储中
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <returns>保存是否成功</returns>
        bool Save(ConfigurationBase config);


        /// <summary>
        /// 从仓储中获取配置对象
        /// </summary>
        /// <param name="configKey">用于标识配置的关键字</param>
        /// <param name="configType">配置类型</param>
        /// <returns>配置对象</returns>
        ConfigurationBase Get(string configKey, Type configType);


        /// <summary>
        /// 删除配置
        /// </summary>
        /// <param name="configKey">用于标识配置的关键字</param>
        void DeleteConfig(string configKey);
    }
}
