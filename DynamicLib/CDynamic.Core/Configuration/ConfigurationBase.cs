using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Core.Configuration
{
    /// <summary>
    /// 基于仓储模式的配置类
    /// </summary>
#if !NETCOREAPP
  [Serializable]
#endif
    public abstract class ConfigurationBase : IConfiguration
    {
        [NonSerialized]
        private IConfigurationRepository repository = null;

        /// <summary>
        /// 配置类型的标识关键字，默认为配置类名称，可重载
        /// </summary>
        public virtual string ConfigKey
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        public ConfigurationBase(IConfigurationRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("配置仓储不可为空![repository]");
            }
            this.repository = repository;
        }

        #region IConfiguration 成员

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns>是否保存成功</returns>
        public virtual bool Save()
        {
            return repository.Save(this);
        }

        /// <summary>
        /// 从仓储中获取配置对象
        /// </summary>
        public virtual void Get()
        {
            ConfigurationBase config = repository.Get(ConfigKey, this.GetType());
            if (config == null)
            {
                config = GetDefaultConfig() as ConfigurationBase;
            }

            if (config == null)
            {
                throw new InvalidOperationException("无法获取配置数据，且未设置默认配置数据");
            }

            CopyFrom(config);
        }

        /// <summary>
        /// 清除配置
        /// </summary>
        public virtual void Clear()
        {
            repository.DeleteConfig(ConfigKey);
        }

        /// <summary>
        /// 获取默认配置数据
        /// </summary>
        /// <returns></returns>
        public abstract IConfiguration GetDefaultConfig();
       

        #endregion

        #region ICloneable 成员
        /// <summary>
        /// 深度复制
        /// </summary>
        /// <returns>新的配置对象</returns>
        public abstract object Clone();


        /// <summary>
        /// 将指定的配置对象属性拷贝到当前实例中
        /// </summary>
        /// <param name="sourceConfig">原配置</param>
        public abstract void CopyFrom(ConfigurationBase sourceConfig);

        #endregion
    }
}
