using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Dynamic.Core.Runtime;
using Dynamic.Core;

namespace Dynamic.Core.Configuration
{
    /// <summary>
    /// 基于二进制文件的仓储
    /// </summary>
    public class ConfigurationBinaryFileRepository : IConfigurationRepository
    {
        /// <summary>
        /// 保存配置文件的目录
        /// </summary>
        public static string ConfigDirectoryBase = Path.Combine(AppExtions.CurrentBaseDirectory, "Config");

        #region IConfigurationRepository 成员

        public bool Save(ConfigurationBase config)
        {
            if (String.IsNullOrEmpty(config.ConfigKey))
            {
                throw new NotSupportedException("必须指定配置对象的配置标识[ConfigKey]");
            }

            if (!(config is BinaryFileConfigurationBase))
            {
                throw new NotSupportedException("传入的配置必须是FileConfigurationBase的子类");
            }

            if (!Directory.Exists(ConfigDirectoryBase))
            {
                Directory.CreateDirectory(ConfigDirectoryBase);
            }

            BinaryFileConfigurationBase fileConfig = config as BinaryFileConfigurationBase;
            string configFileName = Path.Combine(ConfigDirectoryBase, config.ConfigKey + ".cfg");
            using (FileStream fileStream = new FileStream(configFileName, FileMode.Create))
            {
                SerializationUtility.ToBytes(fileConfig, fileStream);
            }
            return true;
        }


        public ConfigurationBase Get(string configKey, Type configType)
        {
            string configFileName = Path.Combine(ConfigDirectoryBase, configKey + ".cfg");

            if (!File.Exists(configFileName))
            {
                return null;
            }

            object configObject = null;
            using (FileStream fileStream = new FileStream(configFileName, FileMode.Open, FileAccess.Read))
            {
                configObject = SerializationUtility.BytesToObject(fileStream);
            }

            return configObject as ConfigurationBase;
        }

        public void DeleteConfig(string configKey)
        {
            string configFileName = Path.Combine(ConfigDirectoryBase, configKey + ".cfg");
            if (File.Exists(configFileName))
            {
                File.Delete(configFileName);
            }
        }
        #endregion
    }
}
