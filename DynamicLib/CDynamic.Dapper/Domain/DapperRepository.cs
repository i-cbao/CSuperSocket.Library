
using System;
using System.Collections.Generic;

using Dynamic.Core.Entities;
using Dynamic.Core.Config;
using Dynamic.Core.ViewModel;
using Dapper;
using System.Data;
using CDynamic.Dapper.Adapters;
using Dynamic.Core;

namespace CDynamic.Dapper.Domain
{
    /// <summary> 基础仓储 </summary>
    /// <typeparam name="T"></typeparam>
    public class DapperRepository<T> : DRepository
        where T : IEntity
    {
        /// <summary> 构造 </summary>
        /// <param name="connectionName"></param>
        public DapperRepository(DBConfig dBConfig) : base(dBConfig)
        {
        }
        public DapperRepository(DBCfgViewModel dBCfgViewModel) : base(dBCfgViewModel)
        {
        }
        /// <summary> 查询所有数据 </summary>
        public IEnumerable<T> Query()
        {
            return Connection.QueryAll<T>();
        }
        /// <summary> 查询所有数据 </summary>
        public IEnumerable<T> Query(string sql, bool isFormatSql=true,object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            if(isFormatSql)
            {
                sql = Connection.FormatSql(sql);
            }
            return Connection.Query<T>(sql,param,transaction,buffered,commandTimeout,commandType);
        }
        public string FormatSql(string sql)
        {
            return Connection.FormatSql(sql);
        }
        public IEnumerable<T> OriQuery(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }
        /// <summary> 根据主键查询单条 </summary>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public T QueryById(object key, string keyColumn = null)
        {
            return Connection.QueryById<T>(key, keyColumn);
        }
        /// <summary> 插入单条数据,不支持有自增列 </summary>
        /// <param name="model"></param>
        /// <param name="excepts">过滤项(如：自增ID)</param>
        /// <returns></returns>
        public int Insert(T model, string[] excepts = null,string tableName=null)
        {
            return Connection.Insert(model, excepts,tableName);
        }
        public  IPagedList<T> PagedList(string sql, int page, int size,
         object param = null)
        {
            SQL pageSql = sql;
            return pageSql.PagedList<T>(Connection, page, size, param);
        }
        /// <summary> 批量插入 </summary>
        /// <param name="models"></param>
        /// <param name="excepts"></param>
        /// <returns></returns>
        public int Insert(IEnumerable<T> models, string[] excepts = null, string tableName = null)
        {
            return Connection.Insert(models, excepts, tableName);
        }
        public  int Update<T>(T entityToUpdate, string[] updateProps = null,
          IDbTransaction trans = null, int? commandTimeout = null)
        {
            return Connection.Update<T>(entityToUpdate,updateProps,trans,commandTimeout);
        }
        public IEnumerable<T> QueryOriCommand(string sql, bool isFormatSql = true, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (isFormatSql)
            {
                sql = Connection.FormatSql(sql);
            }
            return Connection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行原生sql（多分表的时候，上面的语句缓存会丢失tablename）
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="isFormatSql">是否格式化sql，默认格式化</param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public int ExcuteOriCommand(string sql,bool isFormatSql=true, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (isFormatSql)
            {
                sql = Connection.FormatSql(sql);
            }
            return  Connection.Execute(sql, param, transaction, commandTimeout);
        }
       

        public  int Delete(object value, string keyColumn = null, IDbTransaction trans = null)
        {
            return Connection.Delete<T>(value,keyColumn, trans);
        }
        public int DeleteWhere(string where, object param = null, IDbTransaction trans = null)
        {
            return Connection.DeleteWhere<T>(where, param, trans);
        }
        #region 统计
        public long Count(string column, object value)
        {
            return Connection.Count<T>(column, value);
        }

        public long CountWhere(string where = null, object param = null)
        {
            return Connection.CountWhere<T>(where, param);
        }
        public  bool Exists(string column, object value)
        {
            return Connection.Exists<T>(column, value);
        }
        public long Max(string column, string where = null, object param = null)
        {
            return Connection.Max<T>(column, where, param);
        }
        #endregion



    }
}
