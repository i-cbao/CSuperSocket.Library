using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Core.Configuration
{
    /// <summary>
    /// 配置接口
    /// </summary>
    public interface IConfiguration 
    {
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns>保存是否成功</returns>
        bool Save();

        /// <summary>
        /// 从仓储中获取配置
        /// </summary>
        void Get();

        /// <summary>
        /// 获取默认配置
        /// </summary>
        /// <returns></returns>
        IConfiguration GetDefaultConfig();
    }
}
