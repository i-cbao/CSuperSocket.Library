using System;
using System.Data;

using Dynamic.Core.Config;

namespace CDynamic.Dapper
{
    /// <summary> 数据库连接提供者接口 </summary>
    public interface IDbConnectionProvider
    {
        /// <summary> 获取数据库连接 </summary>
        /// <param name="dBConfig">数据库连接配置</param>
        /// <param name="threadCache">是否启用线程级缓存</param>
        IDbConnection Connection(DBConfig dBConfig, bool threadCache = true);
    }
}
