
using CDynamic.Dapper.Adapters;
using Dynamic.Core.Extensions;
using Npgsql;
using System;
using System.Data;
using Dynamic.Core.ViewModel;

namespace CDynamic.Dapper.PostgreSql
{
    public class PostgreSqlAdapter : IDbConnectionAdapter
    {
        public const DBType Name = DBType.PgSql;

        /// <summary> 适配器名称 </summary>
        public DBType ProviderType => Name;
        /// <summary> 连接类型 </summary>
        public Type ConnectionType => typeof(NpgsqlConnection);

        /// <summary> SQL格式化 </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string FormatSql(string sql)
        {
            return sql.Replace("@", ":").Replace("?", ":").Replace("[", "\"").Replace("]", "\"");
        }

        /// <summary> 构建分页SQL </summary>
        /// <param name="sql"></param>
        /// <param name="columns"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string PageSql(string sql, string columns, string order)
        {
            var countSql = sql.Replace(columns, "COUNT(1) ");
            if (order.IsNotNullOrEmpty())
            {
                countSql = countSql.Replace($" {order}", string.Empty);
            }
            sql =
                $"{sql} LIMIT @size OFFSET @skip;{countSql};";
            return sql;
        }

        /// <summary> 创建数据库连接 </summary>
        /// <returns></returns>
        public IDbConnection Create()
        {
            return NpgsqlFactory.Instance.CreateConnection();
        }
    }
}
