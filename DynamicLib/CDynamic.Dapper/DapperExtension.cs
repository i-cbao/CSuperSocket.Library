
using CDynamic.Dapper.Adapters;
using CDynamic.Dapper.PostgreSql;
using Dapper;
using Dynamic.Core;
using Dynamic.Core.Extensions;
using Dynamic.Core.Serialize;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CDynamic.Dapper
{
    /// <summary> 主键属性 </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }

    /// <summary> Dapper自定义扩展 </summary>
    public static partial class DapperExtension
    {
        #region 私有属性
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypePropsCache =
           new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IDictionary<string, string>> FieldsCache =
            new ConcurrentDictionary<RuntimeTypeHandle, IDictionary<string, string>>();

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> InsertCache =
            new ConcurrentDictionary<RuntimeTypeHandle, string>();

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, KeyValuePair<string, string>> KeyCache =
            new ConcurrentDictionary<RuntimeTypeHandle, KeyValuePair<string, string>>();
        #endregion

        #region 私有方法
        private static List<PropertyInfo> Props(Type modelType)
        {
            if (TypePropsCache.TryGetValue(modelType.TypeHandle, out var props))
                return props.ToList();
            props = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            TypePropsCache.TryAdd(modelType.TypeHandle, props);
            return props.ToList();
        }

        private static KeyValuePair<string, string> GetKey(Type modelType)
        {
            const string id = "id";
            if (KeyCache.TryGetValue(modelType.TypeHandle, out var key))
                return key;
            var props = Props(modelType);
            var attr = modelType.GetCustomAttribute<NamingAttribute>();
            var naming = attr?.NamingType;
            var keyProp = props.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            if (keyProp == null)
            {
                keyProp = props.FirstOrDefault(p =>
                    string.Equals(p.PropName(), id, StringComparison.CurrentCultureIgnoreCase));
            }

            key = new KeyValuePair<string, string>(keyProp?.Name ?? id, keyProp?.PropName(naming) ?? id);
            KeyCache.TryAdd(modelType.TypeHandle, key);
            return key;
        }

        private static Type GetInnerType<T>()
        {
            var type = typeof(T);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                type = type.GetGenericArguments()[0];
            }

            return type;
        }
        #endregion

        /// <summary> 查询到DataSet </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="formatVariable"></param>
        /// <param name="param"></param>
        /// <param name="adapter"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static DataSet QueryDataSet(this IDbConnection conn, string sql, Func<string, string> formatVariable,
            object param = null, IDbDataAdapter adapter = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var ds = new DataSet();
            var wasClosed = conn.State == ConnectionState.Closed;
            if (wasClosed)
                conn.Open();
            var command = conn.CreateCommand();
            if (commandType.HasValue)
                command.CommandType = commandType.Value;
            if (commandTimeout.HasValue)
                command.CommandTimeout = commandTimeout.Value;
            command.CommandText = sql;
            if (param != null)
            {
                var ps = param.GetType().GetProperties();
                foreach (var propertyInfo in ps)
                {
                    var propType = propertyInfo.PropertyType;
                    var value = propertyInfo.GetValue(param);
                    if (propType.IsNullableType() && value == null)
                        continue;
                    var p = command.CreateParameter();
                    p.ParameterName = formatVariable(propertyInfo.Name);
                    p.Value = value;
                    command.Parameters.Add(p);
                }
            }
            adapter.SelectCommand = command;
            adapter.Fill(ds);
            if (wasClosed) conn.Close();
            return ds;
        }

        /// <summary> 字段列表 </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static IDictionary<string, string> TypeProperties(this Type modelType)
        {
            if (FieldsCache.TryGetValue(modelType.TypeHandle, out var dict))
                return dict;
            var attr = modelType.GetCustomAttribute<NamingAttribute>();
            var naming = attr?.NamingType;
            var props = Props(modelType);
            var fields = props.ToDictionary(k => k.Name, v => v.PropName(naming));
            FieldsCache.TryAdd(modelType.TypeHandle, fields);
            return fields;
        }

        /// <summary> 获取所有列名(as) </summary>
        /// <param name="modelType"></param>
        /// <param name="excepts">排除的字段</param>
        /// <param name="includes">包含的字段</param>
        /// <returns></returns>
        public static string Columns(this Type modelType, string[] excepts = null, string[] includes = null)
        {
            ///增加表前缀，多表同一字段的情况-Dynamic
            var tableName = modelType.PropName();
            var props = modelType.TypeProperties();
            var sb = new StringBuilder();
            foreach (var prop in props)
            {
                if (excepts != null && excepts.Contains(prop.Key))
                    continue;
                if (includes != null && !includes.Contains(prop.Key))
                    continue;
                if (prop.Key.Equals(prop.Value, StringComparison.CurrentCultureIgnoreCase))
                    sb.AppendFormat("{1}.[{0}],", prop.Key,tableName);
                else
                    sb.AppendFormat("{2}.[{0}] AS [{1}],", prop.Value, prop.Key,tableName);
            }
            return sb.ToString().TrimEnd(',');
        }

        /// <summary> 生成insert语句 </summary>
        /// <returns></returns>
        public static string InsertSql(this Type modelType, string[] excepts = null)
        {
            if (InsertCache.TryGetValue(modelType.TypeHandle, out var sql))
                return sql;

            var tableName = modelType.PropName();
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO [{tableName}]");

            var fields = TypeProperties(modelType);
            if (excepts != null && excepts.Any())
                fields = fields.Where(t => !excepts.Contains(t.Key)).ToDictionary(k => k.Key, v => v.Value);
            var fieldSql = string.Join(",", fields.Select(t => $"[{t.Value}]"));
            var paramSql = string.Join(",", fields.Select(t => $"@{t.Key}"));
            sb.Append($" ({fieldSql}) VALUES ({paramSql})");
            sql = sb.ToString();
            InsertCache.TryAdd(modelType.TypeHandle, sql);
            return sql;
        }

        /// <summary> 设置对象属性 </summary>
        /// <param name="model"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetPropValue(this object model, string propName, object value)
        {
            var type = model.GetType();
            var prop = type.GetProperty(propName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
                prop.SetValue(model, value);
        }

        /// <summary> 获取对象属性 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static T PropValue<T>(this object model, string propName)
        {
            var type = model.GetType();
            var prop = type.GetProperty(propName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return prop == null ? default(T) : prop.GetValue(model).CastTo<T>();
        }

        /// <summary> 获取对象属性 </summary>
        /// <param name="model"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string PropValue(this object model, string propName)
        {
            return PropValue<string>(model, propName);
        }

        #region Query
        private static string QueryAllSql<T>()
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var columns = type.Columns();
            var sql = $"SELECT {columns} FROM [{tableName}]";
            return sql;
        }

        private static string QueryByIdSql<T>(string keyColumn = null)
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var columns = type.Columns();
            var keyName = string.IsNullOrWhiteSpace(keyColumn) ? GetKey(type).Value : keyColumn;
            var sql = $"SELECT {columns} FROM [{tableName}] WHERE [{keyName}]=@id";
            return sql;
        }

        /// <summary> 查询所有数据 </summary>
        public static IEnumerable<T> QueryAll<T>(this IDbConnection conn)
        {
            var sql = QueryAllSql<T>();
            sql = conn.FormatSql(sql);
            return conn.Query<T>(sql);
        }

        /// <summary> 根据主键查询单条 </summary>
        /// <param name="conn"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public static T QueryById<T>(this IDbConnection conn, object key, string keyColumn = null)
        {
            var sql = QueryByIdSql<T>(keyColumn);
            sql = conn.FormatSql(sql);
            return conn.QueryFirstOrDefault<T>(sql, new { id = key });
        }

        /// <summary> 分页 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IPagedList<T> PagedList<T>(this IDbConnection conn, string sql, int page, int size,
            object param = null)
        {
            SQL pageSql = sql;
            return pageSql.PagedList<T>(conn, page, size, param);
        }

        #endregion

        #region Insert
        /// <summary> 插入单条数据,不支持有自增列 </summary>
        /// <param name="conn"></param>
        /// <param name="model"></param>
        /// <param name="excepts">过滤项(如：自增ID)</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert<T>(this IDbConnection conn, T model, string[] excepts = null, IDbTransaction trans = null, int? commandTimeout = null)
        {
            var type = typeof(T);
            var sql = type.InsertSql(excepts);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, model, trans, commandTimeout);
        }

        /// <summary> 批量插入 </summary>
        /// <param name="conn"></param>
        /// <param name="models"></param>
        /// <param name="excepts"></param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert<T>(this IDbConnection conn, IEnumerable<T> models, string[] excepts = null, IDbTransaction trans = null, int? commandTimeout = null)
        {
            var type = typeof(T);
            var sql = type.InsertSql(excepts);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, models.ToArray(), trans, commandTimeout);
        }
        #endregion

        #region Update
        private static string UpdateSql<T>(string[] updateProps = null)
        {
            var type = GetInnerType<T>();
            var tableName = type.PropName();
            var props = type.TypeProperties();
            var key = GetKey(type);
            var sb = new StringBuilder();
            sb.Append($"UPDATE [{tableName}] SET ");
            foreach (var prop in props)
            {
                if (prop.Key == key.Key || updateProps != null && !updateProps.Contains(prop.Key))
                    continue;
                sb.Append($"[{prop.Value}]=@{prop.Key}");
            }

            sb.Append($" WHERE [{key.Value}]=@{key.Key}");
            return sb.ToString();
        }

        /// <summary> 更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdate">待更新实体</param>
        /// <param name="updateProps">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<T>(this IDbConnection conn, T entityToUpdate, string[] updateProps = null,
            IDbTransaction trans = null, int? commandTimeout = null)
        {
            var sql = UpdateSql<T>(updateProps);
            return conn.Execute(sql, entityToUpdate, trans, commandTimeout);
        }
        #endregion

        #region Delete
        private static string DeleteSql<T>(string keyColumn = null)
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var keyName = string.IsNullOrWhiteSpace(keyColumn) ? GetKey(type).Value : keyColumn;
            var sql = $"DELETE FROM [{tableName}] WHERE [{keyName}]=@value";
            return sql;
        }

        /// <summary> 删除数据 </summary>
        /// <param name="conn">连接</param>
        /// <param name="value">列值</param>
        /// <param name="keyColumn">列名</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public static int Delete<T>(this IDbConnection conn, object value, string keyColumn = null, IDbTransaction trans = null)
        {
            var sql = DeleteSql<T>(keyColumn);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, new { value }, trans);
        }

        /// <summary> 删除 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static int DeleteWhere<T>(this IDbConnection conn, string where, object param = null, IDbTransaction trans = null)
        {
            var tableName = typeof(T).PropName();
            var sql = $"DELETE FROM [{tableName}] WHERE {where}";
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, param, trans);
        }
        #endregion

        #region Common

        /// <summary> 统计总数 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Count<T>(this IDbConnection conn, string column, object value)
        {
            var tableName = typeof(T).PropName();
            var sql = $"SELECT COUNT(1) FROM [{tableName}] WHERE [{column}]=@value";
            sql = conn.FormatSql(sql);
            return conn.QueryFirstOrDefault<long>(sql, new { value });
        }

        /// <summary> 统计总数 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long CountWhere<T>(this IDbConnection conn, string where = null, object param = null)
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT COUNT(1) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return conn.QueryFirstOrDefault<long>(sqlStr, param);
        }

        /// <summary> 是否存在 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Exists<T>(this IDbConnection conn, string column, object value)
        {
            return conn.Count<T>(column, value) > 0;
        }

        /// <summary> 是否存在 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool ExistsWhere<T>(this IDbConnection conn, string where = null, object param = null)
        {
            return conn.CountWhere<T>(where, param) > 0;
        }

        /// <summary> 最小 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Min<T>(this IDbConnection conn, string column, string where = null, object param = null)
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT MIN([{column}]) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return conn.QueryFirstOrDefault<long>(sqlStr, param);
        }

        /// <summary> 最大 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Max<T>(this IDbConnection conn, string column, string where = null, object param = null)
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT MAX([{column}]) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return conn.QueryFirstOrDefault<long>(sqlStr);
        }

        /// <summary> 自增数据 </summary>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <param name="count"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static int Increment<T>(this IDbConnection conn, string column, object key, string keyColumn = null,
            int count = 1, IDbTransaction trans = null)
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var keyName = string.IsNullOrWhiteSpace(keyColumn) ? GetKey(type).Value : keyColumn;
            var sql = $"UPDATE [{tableName}] SET [{column}]=[{column}] + @count WHERE [{keyName}]=@id";
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, new { id = key, count }, trans);
        }



        /// <summary> 转换DataTable </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="headerFormat"></param>
        /// <param name="tableName"></param>
        /// <param name="excepts"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> data, Func<string, string> headerFormat = null, string tableName = null, string[] excepts = null)
        {
            var type = GetInnerType<T>();
            tableName = string.IsNullOrWhiteSpace(tableName) ? type.PropName() : tableName;
            var dt = new DataTable(tableName);
            var props = Props(type);
            if (excepts != null && excepts.Any())
                props = props.Where(t => !excepts.Contains(t.Name)).ToList();
            foreach (var prop in props)
            {
                var key = headerFormat?.Invoke(prop.Name) ?? prop.Name;
                dt.Columns.Add(key, prop.PropertyType);
            }

            foreach (var item in data)
            {
                var values = new List<object>();
                foreach (var prop in props)
                {
                    values.Add(prop.GetValue(item));
                }
                dt.Rows.Add(values.ToArray());
            }

            return dt;
        }
        #endregion
    }
}
