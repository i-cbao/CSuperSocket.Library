
using CDynamic.Dapper.Adapters;
using Dynamic.Core.Extensions;
using Npgsql;
using System;
using System.Data;
using Dynamic.Core.ViewModel;
using MySql.Data.MySqlClient;

namespace CDynamic.Dapper.PostgreSql
{

    public class MySqlAdapter : IDbConnectionAdapter
    {
        public const DBType Name = DBType.MySql;
        /// <summary> 适配器名称 </summary>
        public DBType ProviderType => Name;
        /// <summary> 连接类型 </summary>
        public Type ConnectionType => typeof(MySqlConnection);

        /// <summary> 格式化SQL </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string FormatSql(string sql)
        {
            return sql.Replace("[", "`").Replace("]", "`").Replace("\"", "`");
        }

        /// <summary> 构造分页SQL </summary>
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
                $"{sql} LIMIT @skip,@size;{countSql};";
            return sql;
        }

        /// <summary> 创建数据库连接 </summary>
        /// <returns></returns>
        public IDbConnection Create()
        {
            return MySqlClientFactory.Instance.CreateConnection();
        }
    }
   
}
