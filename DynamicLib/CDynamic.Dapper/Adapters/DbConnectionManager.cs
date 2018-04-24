
using CDynamic.Dapper.PostgreSql;
using Dynamic.Core.Exceptions;
using Dynamic.Core.Log;
using System.Collections.Concurrent;
using System.Data;

using Dynamic.Core.ViewModel;

namespace CDynamic.Dapper.Adapters
{
    public static class DbConnectionManager
    {
        private static readonly ConcurrentDictionary<DBType, IDbConnectionAdapter> Adapters;
        private static readonly ILogger Logger;

        static DbConnectionManager()
        {
            Adapters = new ConcurrentDictionary<DBType, IDbConnectionAdapter>();
            AddAdapter(new PostgreSqlAdapter());
            AddAdapter(new MySqlAdapter());
            Logger = LoggerManager.GetLogger(nameof(DbConnectionManager));
        }

        /// <summary> 添加适配器 </summary>
        /// <param name="adapter"></param>
        public static void AddAdapter(IDbConnectionAdapter adapter)
        {
            if (Adapters.ContainsKey(adapter.ProviderType))
                return;
            Adapters.TryAdd(adapter.ProviderType, adapter);
        }

        /// <summary> 创建数据库适配器 </summary>
        /// <param name="providerType">适配器类型</param>
        /// <returns></returns>
        public static IDbConnectionAdapter Create(DBType providerType=DBType.PgSql)
        {
            if (Adapters.TryGetValue(providerType,out var adapter))
                return adapter;
            throw new BusiException($"不支持的DbProvider："+ providerType);
        }

        /// <summary> 格式化SQL </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string FormatSql(this IDbConnection conn, string sql)
        {
            foreach (var adapter in Adapters.Values)
            {
                if (adapter.ConnectionType != conn.GetType())
                    continue;
                sql = adapter.FormatSql(sql);
                Logger.Debug(sql);
                return sql;
            }
            return sql;
        }

        /// <summary> 生成分页SQL </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="columns"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static string PagedSql(this IDbConnection conn, string sql, string columns, string order)
        {
            foreach (var adapter in Adapters.Values)
            {
                if (adapter.ConnectionType == conn.GetType())
                    return adapter.FormatSql(adapter.PageSql(sql, columns, order));
            }
            return sql;
        }
    }
}
